
var hljs = {};
// Convenience variables for build-in objects
var ArrayProto = [],
    objectKeys = Object.keys;

// Global internal variables used within the highlight.js library.
var languages = {},
    aliases = {};

// Regular expressions used throughout the highlight.js library.
var noHighlightRe = /^(no-?highlight|plain|text)$/i,
    languagePrefixRe = /\blang(?:uage)?-([\w-]+)\b/i,
    fixMarkupRe = /((^(<[^>]+>|\t|)+|(?:\n)))/gm;

var spanEndTag = '</span>';

// Global options used when within external APIs. This is modified when
// calling the `hljs.configure` function.
var options = {
    classPrefix: 'hljs-',
    tabReplace: null,
    useBR: false,
    languages: undefined
};

// Object map that is used to escape some common HTML characters.
var escapeRegexMap = {
    '&': '&amp;',
    '<': '&lt;',
    '>': '&gt;'
};

/* Utility functions */

function escape(value) {
    return value.replace(/[&<>]/gm, function (character) {
        return escapeRegexMap[character];
    });
}

function tag(node) {
    return node.nodeName.toLowerCase();
}

function testRe(re, lexeme) {
    var match = re && re.exec(lexeme);
    return match && match.index === 0;
}

function isNotHighlighted(language) {
    return noHighlightRe.test(language);
}

function blockLanguage(block) {
    var i, match, length, _class;
    var classes = block.className + ' ';

    classes += block.parentNode ? block.parentNode.className : '';

    // language-* takes precedence over non-prefixed class names.
    match = languagePrefixRe.exec(classes);
    if (match) {
        return getLanguage(match[1]) ? match[1] : 'no-highlight';
    }

    classes = classes.split(/\s+/);

    for (i = 0, length = classes.length; i < length; i++) {
        _class = classes[i]

        if (isNotHighlighted(_class) || getLanguage(_class)) {
            return _class;
        }
    }
}

function inherit(parent, obj) {
    var key;
    var result = {};

    for (key in parent)
        result[key] = parent[key];
    if (obj)
        for (key in obj)
            result[key] = obj[key];
    return result;
}

/* Stream merging */

function nodeStream(node) {
    var result = [];
    (function _nodeStream(node, offset) {
        for (var child = node.firstChild; child; child = child.nextSibling) {
            if (child.nodeType === 3)
                offset += child.nodeValue.length;
            else if (child.nodeType === 1) {
                result.push({
                    event: 'start',
                    offset: offset,
                    node: child
                });
                offset = _nodeStream(child, offset);
                // Prevent void elements from having an end tag that would actually
                // double them in the output. There are more void elements in HTML
                // but we list only those realistically expected in code display.
                if (!tag(child).match(/br|hr|img|input/)) {
                    result.push({
                        event: 'stop',
                        offset: offset,
                        node: child
                    });
                }
            }
        }
        return offset;
    })(node, 0);
    return result;
}

function mergeStreams(original, highlighted, value) {
    var processed = 0;
    var result = '';
    var nodeStack = [];

    function selectStream() {
        if (!original.length || !highlighted.length) {
            return original.length ? original : highlighted;
        }
        if (original[0].offset !== highlighted[0].offset) {
            return (original[0].offset < highlighted[0].offset) ? original : highlighted;
        }

        /*
        To avoid starting the stream just before it should stop the order is
        ensured that original always starts first and closes last:
    
        if (event1 == 'start' && event2 == 'start')
          return original;
        if (event1 == 'start' && event2 == 'stop')
          return highlighted;
        if (event1 == 'stop' && event2 == 'start')
          return original;
        if (event1 == 'stop' && event2 == 'stop')
          return highlighted;
    
        ... which is collapsed to:
        */
        return highlighted[0].event === 'start' ? original : highlighted;
    }

    function open(node) {
        function attr_str(a) { return ' ' + a.nodeName + '="' + escape(a.value) + '"'; }
        result += '<' + tag(node) + ArrayProto.map.call(node.attributes, attr_str).join('') + '>';
    }

    function close(node) {
        result += '</' + tag(node) + '>';
    }

    function render(event) {
        (event.event === 'start' ? open : close)(event.node);
    }

    while (original.length || highlighted.length) {
        var stream = selectStream();
        result += escape(value.substring(processed, stream[0].offset));
        processed = stream[0].offset;
        if (stream === original) {
            /*
            On any opening or closing tag of the original markup we first close
            the entire highlighted node stack, then render the original tag along
            with all the following original tags at the same offset and then
            reopen all the tags on the highlighted stack.
            */
            nodeStack.reverse().forEach(close);
            do {
                render(stream.splice(0, 1)[0]);
                stream = selectStream();
            } while (stream === original && stream.length && stream[0].offset === processed);
            nodeStack.reverse().forEach(open);
        } else {
            if (stream[0].event === 'start') {
                nodeStack.push(stream[0].node);
            } else {
                nodeStack.pop();
            }
            render(stream.splice(0, 1)[0]);
        }
    }
    return result + escape(value.substr(processed));
}

/* Initialization */


function compileLanguage(language) {

    function reStr(re) {
        return (re && re.source) || re;
    }

    function langRe(value, global) {
        return new RegExp(
            reStr(value),
            'm' + (language.case_insensitive ? 'i' : '') + (global ? 'g' : '')
        );
    }

    let cnt = 0;
    function compileMode(mode, parent) {
        cnt++;
        if (cnt > 5) {
            return;
        }
        if (mode.compiled)
            return;
        mode.compiled = true;

        mode.keywords = mode.keywords || mode.beginKeywords;
        if (mode.keywords) {
            var compiled_keywords = {};

            var flatten = function (className, str) {
                if (language.case_insensitive) {
                    str = str.toLowerCase();
                }
                str.split(' ').forEach(function (kw) {
                    var pair = kw.split('|');
                    compiled_keywords[pair[0]] = [className, pair[1] ? Number(pair[1]) : 1];
                });
            };

            if (typeof mode.keywords === 'string') { // string
                flatten('keyword', mode.keywords);
            } else {
                objectKeys(mode.keywords).forEach(function (className) {
                    flatten(className, mode.keywords[className]);
                });
            }
            mode.keywords = compiled_keywords;
        }
        mode.lexemesRe = langRe(mode.lexemes || /\w+/, true);

        if (parent) {
            if (mode.beginKeywords) {
                mode.begin = '\\b(' + mode.beginKeywords.split(' ').join('|') + ')\\b';
            }
            if (!mode.begin)
                mode.begin = /\B|\b/;
            mode.beginRe = langRe(mode.begin);
            if (!mode.end && !mode.endsWithParent)
                mode.end = /\B|\b/;
            if (mode.end)
                mode.endRe = langRe(mode.end);
            mode.terminator_end = reStr(mode.end) || '';
            if (mode.endsWithParent && parent.terminator_end)
                mode.terminator_end += (mode.end ? '|' : '') + parent.terminator_end;
        }
        if (mode.illegal)
            mode.illegalRe = langRe(mode.illegal);
        if (mode.relevance == null)
            mode.relevance = 1;
        if (!mode.contains) {
            mode.contains = [];
        }
        var expanded_contains = [];
        log("mode.contains.length:" + mode.contains.length);
        mode.contains.forEach(function (c) {
            if (c.variants) {
                c.variants.forEach(function (v) { expanded_contains.push(inherit(c, v)); });
            } else {
                expanded_contains.push(c === 'self' ? mode : c);
            }
        });
        mode.contains = expanded_contains;
        mode.contains.forEach(function (c) {
            compileMode(c, mode);
        });

        if (mode.starts) {
            compileMode(mode.starts, parent);
        }

        var terminators =
            mode.contains.map(function (c) {
                return c.beginKeywords ? '\\.?(' + c.begin + ')\\.?' : c.begin;
            })
                .concat([mode.terminator_end, mode.illegal])
                .map(reStr)
                .filter(Boolean);
        mode.terminators = terminators.length ? langRe(terminators.join('|'), true) : { exec: function (/*s*/) { return null; } };
    }

    compileMode(language);
}

/*
Core highlighting function. Accepts a language name, or an alias, and a
string with the code to highlight. Returns an object with the following
properties:

- relevance (int)
- value (an HTML string with highlighting markup)

*/
function highlight(name, value, ignore_illegals, continuation) {

    function subMode(lexeme, mode) {
        var i, length;

        for (i = 0, length = mode.contains.length; i < length; i++) {
            if (testRe(mode.contains[i].beginRe, lexeme)) {
                return mode.contains[i];
            }
        }
    }

    function endOfMode(mode, lexeme) {
        if (testRe(mode.endRe, lexeme)) {
            while (mode.endsParent && mode.parent) {
                mode = mode.parent;
            }
            return mode;
        }
        if (mode.endsWithParent) {
            return endOfMode(mode.parent, lexeme);
        }
    }

    function isIllegal(lexeme, mode) {
        return !ignore_illegals && testRe(mode.illegalRe, lexeme);
    }

    function keywordMatch(mode, match) {
        var match_str = language.case_insensitive ? match[0].toLowerCase() : match[0];
        return mode.keywords.hasOwnProperty(match_str) && mode.keywords[match_str];
    }

    function buildSpan(classname, insideSpan, leaveOpen, noPrefix) {
        var classPrefix = noPrefix ? '' : options.classPrefix,
            openSpan = '<span class="' + classPrefix,
            closeSpan = leaveOpen ? '' : spanEndTag

        openSpan += classname + '">';

        return openSpan + insideSpan + closeSpan;
    }

    function processKeywords() {
        var keyword_match, last_index, match, result;

        if (!top.keywords)
            return escape(mode_buffer);

        result = '';
        last_index = 0;
        top.lexemesRe.lastIndex = 0;
        match = top.lexemesRe.exec(mode_buffer);

        while (match) {
            result += escape(mode_buffer.substring(last_index, match.index));
            keyword_match = keywordMatch(top, match);
            if (keyword_match) {
                relevance += keyword_match[1];
                result += buildSpan(keyword_match[0], escape(match[0]));
            } else {
                result += escape(match[0]);
            }
            last_index = top.lexemesRe.lastIndex;
            match = top.lexemesRe.exec(mode_buffer);
        }
        return result + escape(mode_buffer.substr(last_index));
    }

    function processSubLanguage() {
        var explicit = typeof top.subLanguage === 'string';
        if (explicit && !languages[top.subLanguage]) {
            return escape(mode_buffer);
        }

        var result = explicit ?
            highlight(top.subLanguage, mode_buffer, true, continuations[top.subLanguage]) :
            highlightAuto(mode_buffer, top.subLanguage.length ? top.subLanguage : undefined);

        // Counting embedded language score towards the host language may be disabled
        // with zeroing the containing mode relevance. Usecase in point is Markdown that
        // allows XML everywhere and makes every XML snippet to have a much larger Markdown
        // score.
        if (top.relevance > 0) {
            relevance += result.relevance;
        }
        if (explicit) {
            continuations[top.subLanguage] = result.top;
        }
        return buildSpan(result.language, result.value, false, true);
    }

    function processBuffer() {
        result += (top.subLanguage != null ? processSubLanguage() : processKeywords());
        mode_buffer = '';
    }

    function startNewMode(mode) {
        result += mode.className ? buildSpan(mode.className, '', true) : '';
        top = Object.create(mode, { parent: { value: top } });
    }

    function processLexeme(buffer, lexeme) {

        mode_buffer += buffer;

        if (lexeme == null) {
            processBuffer();
            return 0;
        }

        var new_mode = subMode(lexeme, top);
        if (new_mode) {
            if (new_mode.skip) {
                mode_buffer += lexeme;
            } else {
                if (new_mode.excludeBegin) {
                    mode_buffer += lexeme;
                }
                processBuffer();
                if (!new_mode.returnBegin && !new_mode.excludeBegin) {
                    mode_buffer = lexeme;
                }
            }
            startNewMode(new_mode, lexeme);
            return new_mode.returnBegin ? 0 : lexeme.length;
        }

        var end_mode = endOfMode(top, lexeme);
        if (end_mode) {
            var origin = top;
            if (origin.skip) {
                mode_buffer += lexeme;
            } else {
                if (!(origin.returnEnd || origin.excludeEnd)) {
                    mode_buffer += lexeme;
                }
                processBuffer();
                if (origin.excludeEnd) {
                    mode_buffer = lexeme;
                }
            }
            do {
                if (top.className) {
                    result += spanEndTag;
                }
                if (!top.skip) {
                    relevance += top.relevance;
                }
                top = top.parent;
            } while (top !== end_mode.parent);
            if (end_mode.starts) {
                startNewMode(end_mode.starts, '');
            }
            return origin.returnEnd ? 0 : lexeme.length;
        }

        if (isIllegal(lexeme, top))
            throw new Error('Illegal lexeme "' + lexeme + '" for mode "' + (top.className || '<unnamed>') + '"');

        /*
        Parser should not reach this point as all types of lexemes should be caught
        earlier, but if it does due to some bug make sure it advances at least one
        character forward to prevent infinite looping.
        */
        mode_buffer += lexeme;
        return lexeme.length || 1;
    }

    var language = getLanguage(name);
    log(name + " " + language);
    if (!language) {
        throw new Error('Unknown language: "' + name + '"');
    }

    log("language：" + language);
    compileLanguage(language);
    var top = continuation || language;
   

    var continuations = {}; // keep continuations for sub-languages
    var result = '', current;
    for (current = top; current !== language; current = current.parent) {
        if (current.className) {
            result = buildSpan(current.className, '', true) + result;
        }
    }
    log("name：" + name);
    var mode_buffer = '';
    var relevance = 0;
    try {
        var match, count, index = 0;
        while (true) {
            log("index："+index);
            top.terminators.lastIndex = index;
            match = top.terminators.exec(value);
            if (!match)
                break;
            count = processLexeme(value.substring(index, match.index), match[0]);
            index = match.index + count;
        }
        processLexeme(value.substr(index));
        for (current = top; current.parent; current = current.parent) { // close dangling modes
            if (current.className) {
                result += spanEndTag;
            }
        }
        return {
            relevance: relevance,
            value: result,
            language: name,
            top: top
        };
    } catch (e) {
        if (e.message && e.message.indexOf('Illegal') !== -1) {
            return {
                relevance: 0,
                value: escape(value)
            };
        } else {
            throw e;
        }
    }
}

/*
Highlighting with language detection. Accepts a string with the code to
highlight. Returns an object with the following properties:

- language (detected language)
- relevance (int)
- value (an HTML string with highlighting markup)
- second_best (object with the same structure for second-best heuristically
  detected language, may be absent)

*/
function highlightAuto(text, languageSubset) {
    languageSubset = languageSubset || options.languages || objectKeys(languages);
    var result = {
        relevance: 0,
        value: escape(text)
    };
    var second_best = result;
    languageSubset.filter(getLanguage).forEach(function (name) {
        var current = highlight(name, text, false);
        current.language = name;
        if (current.relevance > second_best.relevance) {
            second_best = current;
        }
        if (current.relevance > result.relevance) {
            second_best = result;
            result = current;
        }
    });
    if (second_best.language) {
        result.second_best = second_best;
    }
    return result;
}

/*
Post-processing of the highlighted markup:

- replace TABs with something more useful
- replace real line-breaks with '<br>' for non-pre containers

*/
function fixMarkup(value) {
    return !(options.tabReplace || options.useBR)
        ? value
        : value.replace(fixMarkupRe, function (match, p1) {
            if (options.useBR && match === '\n') {
                return '<br>';
            } else if (options.tabReplace) {
                return p1.replace(/\t/g, options.tabReplace);
            }
        });
}

function buildClassName(prevClassName, currentLang, resultLang) {
    var language = currentLang ? aliases[currentLang] : resultLang,
        result = [prevClassName.trim()];

    if (!prevClassName.match(/\bhljs\b/)) {
        result.push('hljs');
    }

    if (prevClassName.indexOf(language) === -1) {
        result.push(language);
    }

    return result.join(' ').trim();
}

/*
Applies highlighting to a DOM node containing code. Accepts a DOM node and
two optional parameters for fixMarkup.
*/
function highlightBlock(block) {
    var node, originalStream, result, resultNode, text;
    var language = blockLanguage(block);

    if (isNotHighlighted(language))
        return;

    if (options.useBR) {
        node = document.createElementNS('http://www.w3.org/1999/xhtml', 'div');
        node.innerHTML = block.innerHTML.replace(/\n/g, '').replace(/<br[ \/]*>/g, '\n');
    } else {
        node = block;
    }
    text = node.textContent;
    result = language ? highlight(language, text, true) : highlightAuto(text);

    originalStream = nodeStream(node);
    if (originalStream.length) {
        resultNode = document.createElementNS('http://www.w3.org/1999/xhtml', 'div');
        resultNode.innerHTML = result.value;
        result.value = mergeStreams(originalStream, nodeStream(resultNode), text);
    }
    result.value = fixMarkup(result.value);

    block.innerHTML = result.value;
    block.className = buildClassName(block.className, language, result.language);
    block.result = {
        language: result.language,
        re: result.relevance
    };
    if (result.second_best) {
        block.second_best = {
            language: result.second_best.language,
            re: result.second_best.relevance
        };
    }
}

/*
Updates highlight.js global options with values passed in the form of an object.
*/
function configure(user_options) {
    options = inherit(options, user_options);
}

/*
Applies highlighting to all <pre><code>..</code></pre> blocks on a page.
*/
function initHighlighting() {
    if (initHighlighting.called)
        return;
    initHighlighting.called = true;

    var blocks = document.querySelectorAll('pre code');
    ArrayProto.forEach.call(blocks, highlightBlock);
}

/*
Attaches highlighting to the page load event.
*/
function initHighlightingOnLoad() {
    addEventListener('DOMContentLoaded', initHighlighting, false);
    addEventListener('load', initHighlighting, false);
}

function registerLanguage(name, language) {
    //log(name + " " + language);
    var lang = languages[name] = language(hljs);
    if (lang.aliases) {
        lang.aliases.forEach(function (alias) { aliases[alias] = name; });
    }
}

function listLanguages() {
    return objectKeys(languages);
}

function getLanguage(name) {
    name = (name || '').toLowerCase();
    return languages[name] || languages[aliases[name]];
}

/* Interface definition */

hljs.highlight = highlight;
hljs.highlightAuto = highlightAuto;
hljs.fixMarkup = fixMarkup;
hljs.highlightBlock = highlightBlock;
hljs.configure = configure;
hljs.initHighlighting = initHighlighting;
hljs.initHighlightingOnLoad = initHighlightingOnLoad;
hljs.registerLanguage = registerLanguage;
hljs.listLanguages = listLanguages;
hljs.getLanguage = getLanguage;
hljs.inherit = inherit;

// Common regexps
hljs.IDENT_RE = '[a-zA-Z]\\w*';
hljs.UNDERSCORE_IDENT_RE = '[a-zA-Z_]\\w*';
hljs.NUMBER_RE = '\\b\\d+(\\.\\d+)?';
hljs.C_NUMBER_RE = '(-?)(\\b0[xX][a-fA-F0-9]+|(\\b\\d+(\\.\\d*)?|\\.\\d+)([eE][-+]?\\d+)?)'; // 0x..., 0..., decimal, float
hljs.BINARY_NUMBER_RE = '\\b(0b[01]+)'; // 0b...
hljs.RE_STARTERS_RE = '!|!=|!==|%|%=|&|&&|&=|\\*|\\*=|\\+|\\+=|,|-|-=|/=|/|:|;|<<|<<=|<=|<|===|==|=|>>>=|>>=|>=|>>>|>>|>|\\?|\\[|\\{|\\(|\\^|\\^=|\\||\\|=|\\|\\||~';

// Common modes
hljs.BACKSLASH_ESCAPE = {
    begin: '\\\\[\\s\\S]', relevance: 0
};
hljs.APOS_STRING_MODE = {
    className: 'string',
    begin: '\'', end: '\'',
    illegal: '\\n',
    contains: [hljs.BACKSLASH_ESCAPE]
};
hljs.QUOTE_STRING_MODE = {
    className: 'string',
    begin: '"', end: '"',
    illegal: '\\n',
    contains: [hljs.BACKSLASH_ESCAPE]
};
hljs.PHRASAL_WORDS_MODE = {
    begin: /\b(a|an|the|are|I'm|isn't|don't|doesn't|won't|but|just|should|pretty|simply|enough|gonna|going|wtf|so|such|will|you|your|like)\b/
};
hljs.COMMENT = function (begin, end, inherits) {
    var mode = hljs.inherit(
        {
            className: 'comment',
            begin: begin, end: end,
            contains: []
        },
        inherits || {}
    );
    mode.contains.push(hljs.PHRASAL_WORDS_MODE);
    mode.contains.push({
        className: 'doctag',
        begin: '(?:TODO|FIXME|NOTE|BUG|XXX):',
        relevance: 0
    });
    return mode;
};
hljs.C_LINE_COMMENT_MODE = hljs.COMMENT('//', '$');
hljs.C_BLOCK_COMMENT_MODE = hljs.COMMENT('/\\*', '\\*/');
hljs.HASH_COMMENT_MODE = hljs.COMMENT('#', '$');
hljs.NUMBER_MODE = {
    className: 'number',
    begin: hljs.NUMBER_RE,
    relevance: 0
};
hljs.C_NUMBER_MODE = {
    className: 'number',
    begin: hljs.C_NUMBER_RE,
    relevance: 0
};
hljs.BINARY_NUMBER_MODE = {
    className: 'number',
    begin: hljs.BINARY_NUMBER_RE,
    relevance: 0
};
hljs.CSS_NUMBER_MODE = {
    className: 'number',
    begin: hljs.NUMBER_RE + '(' +
        '%|em|ex|ch|rem' +
        '|vw|vh|vmin|vmax' +
        '|cm|mm|in|pt|pc|px' +
        '|deg|grad|rad|turn' +
        '|s|ms' +
        '|Hz|kHz' +
        '|dpi|dpcm|dppx' +
        ')?',
    relevance: 0
};
hljs.REGEXP_MODE = {
    className: 'regexp',
    begin: /\//, end: /\/[gimuy]*/,
    illegal: /\n/,
    contains: [
        hljs.BACKSLASH_ESCAPE,
        {
            begin: /\[/, end: /\]/,
            relevance: 0,
            contains: [hljs.BACKSLASH_ESCAPE]
        }
    ]
};
hljs.TITLE_MODE = {
    className: 'title',
    begin: hljs.IDENT_RE,
    relevance: 0
};
hljs.UNDERSCORE_TITLE_MODE = {
    className: 'title',
    begin: hljs.UNDERSCORE_IDENT_RE,
    relevance: 0
};
hljs.METHOD_GUARD = {
    // excludes method names from keyword processing
    begin: '\\.\\s*' + hljs.UNDERSCORE_IDENT_RE,
    relevance: 0
};

hljs.registerLanguage("sql", function (hljs) {
    var COMMENT_MODE = hljs.COMMENT('--', '$');
    return {
        case_insensitive: true,
        illegal: /[<>{}*#]/,
        contains: [
            {
                beginKeywords:
                    'begin end start commit rollback savepoint lock alter create drop rename call ' +
                    'delete do handler insert load replace select truncate update set show pragma grant ' +
                    'merge describe use explain help declare prepare execute deallocate release ' +
                    'unlock purge reset change stop analyze cache flush optimize repair kill ' +
                    'install uninstall checksum restore check backup revoke comment',
                end: /;/, endsWithParent: true,
                lexemes: /[\w\.]+/,
                keywords: {
                    keyword:
                        'abort abs absolute acc acce accep accept access accessed accessible account acos action activate add ' +
                        'addtime admin administer advanced advise aes_decrypt aes_encrypt after agent aggregate ali alia alias ' +
                        'allocate allow alter always analyze ancillary and any anydata anydataset anyschema anytype apply ' +
                        'archive archived archivelog are as asc ascii asin assembly assertion associate asynchronous at atan ' +
                        'atn2 attr attri attrib attribu attribut attribute attributes audit authenticated authentication authid ' +
                        'authors auto autoallocate autodblink autoextend automatic availability avg backup badfile basicfile ' +
                        'before begin beginning benchmark between bfile bfile_base big bigfile bin binary_double binary_float ' +
                        'binlog bit_and bit_count bit_length bit_or bit_xor bitmap blob_base block blocksize body both bound ' +
                        'buffer_cache buffer_pool build bulk by byte byteordermark bytes cache caching call calling cancel ' +
                        'capacity cascade cascaded case cast catalog category ceil ceiling chain change changed char_base ' +
                        'char_length character_length characters characterset charindex charset charsetform charsetid check ' +
                        'checksum checksum_agg child choose chr chunk class cleanup clear client clob clob_base clone close ' +
                        'cluster_id cluster_probability cluster_set clustering coalesce coercibility col collate collation ' +
                        'collect colu colum column column_value columns columns_updated comment commit compact compatibility ' +
                        'compiled complete composite_limit compound compress compute concat concat_ws concurrent confirm conn ' +
                        'connec connect connect_by_iscycle connect_by_isleaf connect_by_root connect_time connection ' +
                        'consider consistent constant constraint constraints constructor container content contents context ' +
                        'contributors controlfile conv convert convert_tz corr corr_k corr_s corresponding corruption cos cost ' +
                        'count count_big counted covar_pop covar_samp cpu_per_call cpu_per_session crc32 create creation ' +
                        'critical cross cube cume_dist curdate current current_date current_time current_timestamp current_user ' +
                        'cursor curtime customdatum cycle data database databases datafile datafiles datalength date_add ' +
                        'date_cache date_format date_sub dateadd datediff datefromparts datename datepart datetime2fromparts ' +
                        'day day_to_second dayname dayofmonth dayofweek dayofyear days db_role_change dbtimezone ddl deallocate ' +
                        'declare decode decompose decrement decrypt deduplicate def defa defau defaul default defaults ' +
                        'deferred defi defin define degrees delayed delegate delete delete_all delimited demand dense_rank ' +
                        'depth dequeue des_decrypt des_encrypt des_key_file desc descr descri describ describe descriptor ' +
                        'deterministic diagnostics difference dimension direct_load directory disable disable_all ' +
                        'disallow disassociate discardfile disconnect diskgroup distinct distinctrow distribute distributed div ' +
                        'do document domain dotnet double downgrade drop dumpfile duplicate duration each edition editionable ' +
                        'editions element ellipsis else elsif elt empty enable enable_all enclosed encode encoding encrypt ' +
                        'end end-exec endian enforced engine engines enqueue enterprise entityescaping eomonth error errors ' +
                        'escaped evalname evaluate event eventdata events except exception exceptions exchange exclude excluding ' +
                        'execu execut execute exempt exists exit exp expire explain export export_set extended extent external ' +
                        'external_1 external_2 externally extract failed failed_login_attempts failover failure far fast ' +
                        'feature_set feature_value fetch field fields file file_name_convert filesystem_like_logging final ' +
                        'finish first first_value fixed flash_cache flashback floor flush following follows for forall force ' +
                        'form forma format found found_rows freelist freelists freepools fresh from from_base64 from_days ' +
                        'ftp full function general generated get get_format get_lock getdate getutcdate global global_name ' +
                        'globally go goto grant grants greatest group group_concat group_id grouping grouping_id groups ' +
                        'gtid_subtract guarantee guard handler hash hashkeys having hea head headi headin heading heap help hex ' +
                        'hierarchy high high_priority hosts hour http id ident_current ident_incr ident_seed identified ' +
                        'identity idle_time if ifnull ignore iif ilike ilm immediate import in include including increment ' +
                        'index indexes indexing indextype indicator indices inet6_aton inet6_ntoa inet_aton inet_ntoa infile ' +
                        'initial initialized initially initrans inmemory inner innodb input insert install instance instantiable ' +
                        'instr interface interleaved intersect into invalidate invisible is is_free_lock is_ipv4 is_ipv4_compat ' +
                        'is_not is_not_null is_used_lock isdate isnull isolation iterate java join json json_exists ' +
                        'keep keep_duplicates key keys kill language large last last_day last_insert_id last_value lax lcase ' +
                        'lead leading least leaves left len lenght length less level levels library like like2 like4 likec limit ' +
                        'lines link list listagg little ln load load_file lob lobs local localtime localtimestamp locate ' +
                        'locator lock locked log log10 log2 logfile logfiles logging logical logical_reads_per_call ' +
                        'logoff logon logs long loop low low_priority lower lpad lrtrim ltrim main make_set makedate maketime ' +
                        'managed management manual map mapping mask master master_pos_wait match matched materialized max ' +
                        'maxextents maximize maxinstances maxlen maxlogfiles maxloghistory maxlogmembers maxsize maxtrans ' +
                        'md5 measures median medium member memcompress memory merge microsecond mid migration min minextents ' +
                        'minimum mining minus minute minvalue missing mod mode model modification modify module monitoring month ' +
                        'months mount move movement multiset mutex name name_const names nan national native natural nav nchar ' +
                        'nclob nested never new newline next nextval no no_write_to_binlog noarchivelog noaudit nobadfile ' +
                        'nocheck nocompress nocopy nocycle nodelay nodiscardfile noentityescaping noguarantee nokeep nologfile ' +
                        'nomapping nomaxvalue nominimize nominvalue nomonitoring none noneditionable nonschema noorder ' +
                        'nopr nopro noprom nopromp noprompt norely noresetlogs noreverse normal norowdependencies noschemacheck ' +
                        'noswitch not nothing notice notrim novalidate now nowait nth_value nullif nulls num numb numbe ' +
                        'nvarchar nvarchar2 object ocicoll ocidate ocidatetime ociduration ociinterval ociloblocator ocinumber ' +
                        'ociref ocirefcursor ocirowid ocistring ocitype oct octet_length of off offline offset oid oidindex old ' +
                        'on online only opaque open operations operator optimal optimize option optionally or oracle oracle_date ' +
                        'oradata ord ordaudio orddicom orddoc order ordimage ordinality ordvideo organization orlany orlvary ' +
                        'out outer outfile outline output over overflow overriding package pad parallel parallel_enable ' +
                        'parameters parent parse partial partition partitions pascal passing password password_grace_time ' +
                        'password_lock_time password_reuse_max password_reuse_time password_verify_function patch path patindex ' +
                        'pctincrease pctthreshold pctused pctversion percent percent_rank percentile_cont percentile_disc ' +
                        'performance period period_add period_diff permanent physical pi pipe pipelined pivot pluggable plugin ' +
                        'policy position post_transaction pow power pragma prebuilt precedes preceding precision prediction ' +
                        'prediction_cost prediction_details prediction_probability prediction_set prepare present preserve ' +
                        'prior priority private private_sga privileges procedural procedure procedure_analyze processlist ' +
                        'profiles project prompt protection public publishingservername purge quarter query quick quiesce quota ' +
                        'quotename radians raise rand range rank raw read reads readsize rebuild record records ' +
                        'recover recovery recursive recycle redo reduced ref reference referenced references referencing refresh ' +
                        'regexp_like register regr_avgx regr_avgy regr_count regr_intercept regr_r2 regr_slope regr_sxx regr_sxy ' +
                        'reject rekey relational relative relaylog release release_lock relies_on relocate rely rem remainder rename ' +
                        'repair repeat replace replicate replication required reset resetlogs resize resource respect restore ' +
                        'restricted result result_cache resumable resume retention return returning returns reuse reverse revoke ' +
                        'right rlike role roles rollback rolling rollup round row row_count rowdependencies rowid rownum rows ' +
                        'rtrim rules safe salt sample save savepoint sb1 sb2 sb4 scan schema schemacheck scn scope scroll ' +
                        'sdo_georaster sdo_topo_geometry search sec_to_time second section securefile security seed segment select ' +
                        'self sequence sequential serializable server servererror session session_user sessions_per_user set ' +
                        'sets settings sha sha1 sha2 share shared shared_pool short show shrink shutdown si_averagecolor ' +
                        'si_colorhistogram si_featurelist si_positionalcolor si_stillimage si_texture siblings sid sign sin ' +
                        'size size_t sizes skip slave sleep smalldatetimefromparts smallfile snapshot some soname sort soundex ' +
                        'source space sparse spfile split sql sql_big_result sql_buffer_result sql_cache sql_calc_found_rows ' +
                        'sql_small_result sql_variant_property sqlcode sqldata sqlerror sqlname sqlstate sqrt square standalone ' +
                        'standby start starting startup statement static statistics stats_binomial_test stats_crosstab ' +
                        'stats_ks_test stats_mode stats_mw_test stats_one_way_anova stats_t_test_ stats_t_test_indep ' +
                        'stats_t_test_one stats_t_test_paired stats_wsr_test status std stddev stddev_pop stddev_samp stdev ' +
                        'stop storage store stored str str_to_date straight_join strcmp strict string struct stuff style subdate ' +
                        'subpartition subpartitions substitutable substr substring subtime subtring_index subtype success sum ' +
                        'suspend switch switchoffset switchover sync synchronous synonym sys sys_xmlagg sysasm sysaux sysdate ' +
                        'sysdatetimeoffset sysdba sysoper system system_user sysutcdatetime table tables tablespace tan tdo ' +
                        'template temporary terminated tertiary_weights test than then thread through tier ties time time_format ' +
                        'time_zone timediff timefromparts timeout timestamp timestampadd timestampdiff timezone_abbr ' +
                        'timezone_minute timezone_region to to_base64 to_date to_days to_seconds todatetimeoffset trace tracking ' +
                        'transaction transactional translate translation treat trigger trigger_nestlevel triggers trim truncate ' +
                        'try_cast try_convert try_parse type ub1 ub2 ub4 ucase unarchived unbounded uncompress ' +
                        'under undo unhex unicode uniform uninstall union unique unix_timestamp unknown unlimited unlock unpivot ' +
                        'unrecoverable unsafe unsigned until untrusted unusable unused update updated upgrade upped upper upsert ' +
                        'url urowid usable usage use use_stored_outlines user user_data user_resources users using utc_date ' +
                        'utc_timestamp uuid uuid_short validate validate_password_strength validation valist value values var ' +
                        'var_samp varcharc vari varia variab variabl variable variables variance varp varraw varrawc varray ' +
                        'verify version versions view virtual visible void wait wallet warning warnings week weekday weekofyear ' +
                        'wellformed when whene whenev wheneve whenever where while whitespace with within without work wrapped ' +
                        'xdb xml xmlagg xmlattributes xmlcast xmlcolattval xmlelement xmlexists xmlforest xmlindex xmlnamespaces ' +
                        'xmlpi xmlquery xmlroot xmlschema xmlserialize xmltable xmltype xor year year_to_month years yearweek',
                    literal:
                        'true false null',
                    built_in:
                        'array bigint binary bit blob boolean char character date dec decimal float int int8 integer interval number ' +
                        'numeric real record serial serial8 smallint text varchar varying void'
                },
                contains: [
                    {
                        className: 'string',
                        begin: '\'', end: '\'',
                        contains: [hljs.BACKSLASH_ESCAPE, { begin: '\'\'' }]
                    },
                    {
                        className: 'string',
                        begin: '"', end: '"',
                        contains: [hljs.BACKSLASH_ESCAPE, { begin: '""' }]
                    },
                    {
                        className: 'string',
                        begin: '`', end: '`',
                        contains: [hljs.BACKSLASH_ESCAPE]
                    },
                    hljs.C_NUMBER_MODE,
                    hljs.C_BLOCK_COMMENT_MODE,
                    COMMENT_MODE
                ]
            },
            hljs.C_BLOCK_COMMENT_MODE,
            COMMENT_MODE
        ]
    };
});