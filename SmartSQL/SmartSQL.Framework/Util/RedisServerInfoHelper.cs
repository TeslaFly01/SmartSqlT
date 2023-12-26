using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSQL.Framework.Util
{
    public class RedisServerInfoHelper
    {
        public static RedisServerInfo ParseServerInfo(string serverInfoString)
        {
            RedisServerInfo serverInfo = new RedisServerInfo();
            serverInfo.Databases = new Dictionary<int, RedisDatabaseInfo>();

            string[] lines = serverInfoString.Split('\n', (char)StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                {
                    // Skip empty lines and comments
                    continue;
                }
                string[] parts = line.Split(':');
                if (parts.Length != 2)
                {
                    // Skip invalid lines
                    continue;
                }
                string key = parts[0].Trim();
                string value = parts[1].Trim();
                try
                {
                    switch (key)
                    {
                        #region MyRegion
                        case "redis_version":
                            serverInfo.RedisVersion = value;
                            break;
                        case "redis_git_sha1":
                            serverInfo.RedisGitSha1 = value;
                            break;
                        case "redis_git_dirty":
                            serverInfo.RedisGitDirty = value;
                            break;
                        case "redis_build_id":
                            serverInfo.RedisBuildId=value;
                            break;
                        case "redis_mode":
                            serverInfo.RedisMode=value;
                            break;
                        case "os":
                            serverInfo.Os=value;
                            break;
                        case "arch_bits":
                            serverInfo.ArchBits=value;
                            break;
                        case "multiplexing_api":
                            serverInfo.MultiplexingApi=value;
                            break;
                        case "atomicvar_api":
                            serverInfo.AtomicvarApi=value;
                            break;
                        case "process_id":
                            serverInfo.ProcessId=value;
                            break;
                        case "run_id":
                            serverInfo.RunId=value;
                            break;
                        case "tcp_port":
                            serverInfo.TcpPort=value;
                            break;
                        case "uptime_in_seconds":
                            serverInfo.UptimeInSeconds=value;
                            break;
                        case "uptime_in_days":
                            serverInfo.UptimeInDays = value;
                            break;
                        case "hz":
                            serverInfo.Hz=value;
                            break;
                        case "configured_hz":
                            serverInfo.ConfiguredHz=value;
                            break;
                        case "lru_clock":
                            serverInfo.LruClock=value;
                            break;
                        case "executable":
                            serverInfo.Executable=value;
                            break;
                        case "connected_clients":
                            serverInfo.ConnectedClients = value;
                            break;
                        case "used_memory":
                            serverInfo.UsedMemory = long.Parse(value);
                            break;
                        case "used_memory_human":
                            serverInfo.UsedMemoryHuman = value;
                            break;
                        case "used_memory_rss":
                            serverInfo.UsedMemoryRss = long.Parse(value);
                            break;
                        case "used_memory_rss_human":
                            serverInfo.UsedMemoryRssHuman = value;
                            break;
                        case "used_memory_peak":
                            serverInfo.UsedMemoryPeak = long.Parse(value);
                            break;
                        case "used_memory_peak_human":
                            serverInfo.UsedMemoryPeakHuman = value;
                            break;
                        case "used_memory_peak_perc":
                            serverInfo.UsedMemoryPeakPerc = value;
                            break;
                        case "used_memory_overhead":
                            serverInfo.UsedMemoryOverhead = long.Parse(value);
                            break;
                        case "used_memory_startup":
                            serverInfo.UsedMemoryStartUp = long.Parse(value);
                            break;
                        case "used_memory_dataset":
                            serverInfo.UsedMemoryDataset = long.Parse(value);
                            break;
                        case "used_memory_dataset_perc":
                            serverInfo.UsedMemoryDatasetPerc = value;
                            break;
                        case "allocator_allocated":
                            serverInfo.AllocatorAllocated = long.Parse(value);
                            break;
                        case "allocator_active":
                            serverInfo.AllocatorActive = long.Parse(value);
                            break;
                        case "allocator_resident":
                            serverInfo.AllocatorResident = long.Parse(value);
                            break;
                        case "total_system_memory":
                            serverInfo.TotalSystemMemory = long.Parse(value);
                            break;
                        case "total_system_memory_human":
                            serverInfo.TotalSystemMemoryHuman = value;
                            break;
                        case "used_memory_lua":
                            serverInfo.UsedMemoryLua = long.Parse(value);
                            break;
                        case "used_memory_lua_human":
                            serverInfo.UsedMemoryLuaHuman = value;
                            break;
                        case "used_memory_scripts":
                            serverInfo.UsedMemoryScripts = long.Parse(value);
                            break;
                        case "used_memory_scripts_human":
                            serverInfo.UsedMemoryScriptsHuman = value;
                            break;
                        case "number_of_cached_scripts":
                            serverInfo.NumberOfCachedScripts = long.Parse(value);
                            break;
                        case "maxmemory":
                            serverInfo.Maxmemory = long.Parse(value);
                            break;
                        case "maxmemory_human":
                            serverInfo.MaxmemoryHuman = value;
                            break;
                        case "maxmemory_policy":
                            serverInfo.MaxmemoryPolicy = value;
                            break;
                        case "allocator_frag_ratio":
                            serverInfo.AllocatorFragRatio = value;
                            break;
                        case "allocator_frag_bytes":
                            serverInfo.AllocatorFragBytes = long.Parse(value);
                            break;
                        case "allocator_rss_ratio":
                            serverInfo.AllocatorRssRatio = value;
                            break;
                        case "allocator_rss_bytes":
                            serverInfo.AllocatorRssBytes = long.Parse(value);
                            break;
                        case "rss_overhead_ratio":
                            serverInfo.RssOverheadRatio = value;
                            break;
                        case "rss_overhead_bytes":
                            serverInfo.RssOverheadBytes = long.Parse(value);
                            break;
                        case "mem_fragmentation_ratio":
                            serverInfo.MemFragmentationRatio = value;
                            break;
                        case "mem_fragmentation_bytes":
                            serverInfo.MemFragmentationBytes = long.Parse(value);
                            break;
                        case "mem_not_counted_for_evict":
                            serverInfo.MemNotCountedForEvict = long.Parse(value);
                            break;
                        case "mem_replication_backlog":
                            serverInfo.MemReplicationBacklog = long.Parse(value);
                            break;
                        case "mem_clients_slaves":
                            serverInfo.MemClientsSlaves = long.Parse(value);
                            break;
                        case "mem_clients_normal":
                            serverInfo.MemClientsNormal = long.Parse(value);
                            break;
                        case "mem_aof_buffer":
                            serverInfo.MemAofBuffer = long.Parse(value);
                            break;
                        case "mem_allocator":
                            serverInfo.MemAllocator = value;
                            break;
                        case "active_defrag_running":
                            serverInfo.ActiveDefragRunning = long.Parse(value);
                            break;
                        case "lazyfree_pending_objects":
                            serverInfo.LazyfreePendingObjects = long.Parse(value);
                            break;
                        case "loading":
                            serverInfo.Loading = long.Parse(value);
                            break;
                        case "rdb_changes_since_last_save":
                            serverInfo.RdbChangesSinceLastSave = long.Parse(value);
                            break;
                        case "rdb_bgsave_in_progress":
                            serverInfo.RdbBgsaveInProgress = long.Parse(value);
                            break;
                        case "rdb_last_save_time":
                            serverInfo.RdbLastSaveTime = long.Parse(value);
                            break;
                        case "rdb_last_bgsave_status":
                            serverInfo.RdbLastBgsaveStatus = value;
                            break;
                        case "rdb_last_bgsave_time_sec":
                            serverInfo.RdbLastBgsaveTimeSec = long.Parse(value);
                            break;
                        case "rdb_current_bgsave_time_sec":
                            serverInfo.RdbCurrentBgsaveTimeSec = long.Parse(value);
                            break;
                        case "rdb_last_cow_size":
                            serverInfo.RdbLastCowSize = long.Parse(value);
                            break;
                        case "aof_enabled":
                            serverInfo.AofEnabled = long.Parse(value);
                            break;
                        case "aof_rewrite_in_progress":
                            serverInfo.AofRewriteInProgress = long.Parse(value);
                            break;
                        case "aof_rewrite_scheduled":
                            serverInfo.AofRewriteScheduled = long.Parse(value);
                            break;
                        case "aof_last_rewrite_time_sec":
                            serverInfo.AofLastRewriteTimeSec = long.Parse(value);
                            break;
                        case "aof_current_rewrite_time_sec":
                            serverInfo.AofCurrentRewriteTimeSec = long.Parse(value);
                            break;
                        case "aof_last_bgrewrite_status":
                            serverInfo.AofLastBgrewriteStatus = value;
                            break;
                        case "aof_last_write_status":
                            serverInfo.AofLastWriteStatus = value;
                            break;
                        case "aof_last_cow_size":
                            serverInfo.AofLastCowSize = long.Parse(value);
                            break;
                        case "total_connections_received":
                            serverInfo.TotalConnectionsReceived = int.Parse(value);
                            break;
                        case "total_commands_processed":
                            serverInfo.TotalCommandsProcessed = int.Parse(value);
                            break;
                        case "instantaneous_ops_per_sec":
                            serverInfo.InstantaneousOpsPerSec = long.Parse(value);
                            break;
                        case "total_net_input_bytes":
                            serverInfo.TotalNetInputBytes = long.Parse(value);
                            break;
                        case "total_net_output_bytes":
                            serverInfo.TotalNetOutputBytes = long.Parse(value);
                            break;
                        case "instantaneous_input_kbps":
                            serverInfo.InstantaneousInputKbps = value;
                            break;
                        case "instantaneous_output_kbps":
                            serverInfo.InstantaneousOutputKbps = value;
                            break;
                        case "rejected_connections":
                            serverInfo.RejectedConnections = long.Parse(value);
                            break;
                        case "sync_full":
                            serverInfo.SyncFull = long.Parse(value);
                            break;
                        case "sync_partial_ok":
                            serverInfo.SyncPartialOk = long.Parse(value);
                            break;
                        case "sync_partial_err":
                            serverInfo.SyncPartialErr = long.Parse(value);
                            break;
                        case "expired_keys":
                            serverInfo.ExpiredKeys = long.Parse(value);
                            break;
                        case "expired_stale_perc":
                            serverInfo.ExpiredStalePerc = value;
                            break;
                        case "expired_time_cap_reached_count":
                            serverInfo.ExpiredTimeCapReachedCount = long.Parse(value);
                            break;
                        case "evicted_keys":
                            serverInfo.EvictedKeys = long.Parse(value);
                            break;
                        case "keyspace_hits":
                            serverInfo.KeyspaceHits = long.Parse(value);
                            break;
                        case "keyspace_misses":
                            serverInfo.KeyspaceMisses = long.Parse(value);
                            break;
                        case "pubsub_channels":
                            serverInfo.PubsubChannels = long.Parse(value);
                            break;
                        case "pubsub_patterns":
                            serverInfo.PubsubPatterns = long.Parse(value);
                            break;
                        case "latest_fork_usec":
                            serverInfo.LatestForkUsec = long.Parse(value);
                            break;
                        case "migrate_cached_sockets":
                            serverInfo.MigrateCachedSockets = long.Parse(value);
                            break;
                        case "slave_expires_tracked_keys":
                            serverInfo.SlaveExpiresTrackedKeys = long.Parse(value);
                            break;
                        case "active_defrag_hits":
                            serverInfo.ActiveDefragHits = long.Parse(value);
                            break;
                        case "active_defrag_misses":
                            serverInfo.ActiveDefragMisses = long.Parse(value);
                            break;
                        case "active_defrag_key_hits":
                            serverInfo.ActiveDefragKeyHits = long.Parse(value);
                            break;
                        case "active_defrag_key_misses":
                            serverInfo.ActiveDefragKeyMisses = long.Parse(value);
                            break;
                        case "role":
                            serverInfo.Role = value;
                            break;
                        case "connected_slaves":
                            serverInfo.ConnectedSlaves = long.Parse(value);
                            break;
                        case "master_replid":
                            serverInfo.MasterReplid = value;
                            break;
                        case "master_replid2":
                            serverInfo.MasterReplid2 = value;
                            break;
                        case "master_repl_offset":
                            serverInfo.MasterReplOffset = long.Parse(value);
                            break;
                        case "second_repl_offset":
                            serverInfo.SecondReplOffset = long.Parse(value);
                            break;
                        case "repl_backlog_active":
                            serverInfo.ReplBacklogActive = long.Parse(value);
                            break;
                        case "repl_backlog_size":
                            serverInfo.ReplBacklogSize = long.Parse(value);
                            break;
                        case "repl_backlog_first_byte_offset":
                            serverInfo.ReplBacklogFirstByteOffset = long.Parse(value);
                            break;
                        case "repl_backlog_histlen":
                            serverInfo.ReplBacklogHistlen = long.Parse(value);
                            break;
                        case "used_cpu_sys":
                            serverInfo.UsedCpuSys = value;
                            break;
                        case "used_cpu_sys_children":
                            serverInfo.UsedCpuSysChildren = value;
                            break;
                        case "used_cpu_user":
                            serverInfo.UsedCpuUser = value;
                            break;
                        case "used_cpu_user_children":
                            serverInfo.UsedCpuUserChildren = value;
                            break;
                        case "cluster_enabled":
                            serverInfo.ClusterEnabled = value;
                            break;
                        default:
                            if (key.StartsWith("db"))
                            {
                                int dbIndex = int.Parse(key.Replace("db", ""));
                                RedisDatabaseInfo databaseInfo = ParseDatabaseInfo(value);
                                serverInfo.Databases.Add(dbIndex, databaseInfo);
                            }
                            break; 
                            #endregion
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return serverInfo;
        }

        public static RedisDatabaseInfo ParseDatabaseInfo(string databaseInfoString)
        {
            RedisDatabaseInfo databaseInfo = new RedisDatabaseInfo();

            string[] parts = databaseInfoString.Split(',', (char)StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
            {
                string[] keyValue = part.Split('=');
                if (keyValue.Length != 2)
                {
                    continue;
                }

                string key = keyValue[0].Trim();
                string value = keyValue[1].Trim();

                switch (key)
                {
                    case "keys":
                        databaseInfo.Keys = int.Parse(value);
                        break;
                    case "expires":
                        databaseInfo.Expires = int.Parse(value);
                        break;
                    case "avg_ttl":
                        databaseInfo.AvgTtl = long.Parse(value);
                        break;
                }
            }

            return databaseInfo;
        }
    }

    public class RedisServerInfo
    {
        public string RedisVersion { get; set; }
        public string RedisGitSha1 { get; set; }
        public string RedisGitDirty { get; set; }
        public string RedisBuildId { get; set; }
        public string RedisMode { get; set; }
        public string Os { get; set; }
        public string ArchBits { get; set; }
        public string MultiplexingApi { get; set; }
        public string AtomicvarApi { get; set; }
        public string ProcessId { get; set; }
        public string RunId { get; set; }
        public string TcpPort { get; set; }
        public string UptimeInSeconds { get; set; }
        public string UptimeInDays { get; set; }
        public string Hz { get; set; }
        public string ConfiguredHz { get; set; }
        public string LruClock { get; set; }
        public string Executable { get; set; }
        public string ConnectedClients { get; set; }
        public long UsedMemory { get; set; }
        public string UsedMemoryHuman { get; set; }
        public long UsedMemoryRss { get; set; }
        public string UsedMemoryRssHuman { get; set; }
        public long UsedMemoryPeak { get; set; }
        public string UsedMemoryPeakHuman { get; set; }
        public string UsedMemoryPeakPerc { get; set; }
        public long UsedMemoryOverhead { get; set; }
        public long UsedMemoryStartUp { get; set; }
        public long UsedMemoryDataset { get; set; }
        public string UsedMemoryDatasetPerc { get; set; }
        public long AllocatorAllocated { get; set; }
        public long AllocatorActive { get; set; }
        public long AllocatorResident { get; set; }
        public long TotalSystemMemory { get; set; }
        public string TotalSystemMemoryHuman { get; set; }
        public long UsedMemoryLua { get; set; }
        public string UsedMemoryLuaHuman { get; set; }
        public long UsedMemoryScripts { get; set; }
        public string UsedMemoryScriptsHuman { get; set; }
        public long NumberOfCachedScripts { get; set; }
        public long Maxmemory { get; set; }
        public string MaxmemoryHuman { get; set; }
        public string MaxmemoryPolicy { get; set; }
        public string AllocatorFragRatio { get; set; }
        public long AllocatorFragBytes { get; set; }
        public string AllocatorRssRatio { get; set; }
        public long AllocatorRssBytes { get; set; }
        public string RssOverheadRatio { get; set; }
        public long RssOverheadBytes { get; set; }
        public string MemFragmentationRatio { get; set; }
        public long MemFragmentationBytes { get; set; }
        public long MemNotCountedForEvict { get; set; }
        public long MemReplicationBacklog { get; set; }
        public long MemClientsSlaves { get; set; }
        public long MemClientsNormal { get; set; }
        public long MemAofBuffer { get; set; }
        public string MemAllocator { get; set; }
        public long ActiveDefragRunning { get; set; }
        public long LazyfreePendingObjects { get; set; }
        public long Loading { get; set; }
        public long RdbChangesSinceLastSave { get; set; }
        public long RdbBgsaveInProgress { get; set; }
        public long RdbLastSaveTime { get; set; }
        public string RdbLastBgsaveStatus { get; set; }
        public long RdbLastBgsaveTimeSec { get; set; }
        public long RdbCurrentBgsaveTimeSec { get; set; }
        public long RdbLastCowSize { get; set; }
        public long AofEnabled { get; set; }
        public long AofRewriteInProgress { get; set; }
        public long AofRewriteScheduled { get; set; }
        public long AofLastRewriteTimeSec { get; set; }
        public long AofCurrentRewriteTimeSec { get; set; }
        public string AofLastBgrewriteStatus { get; set; }
        public string AofLastWriteStatus { get; set; }
        public long AofLastCowSize { get; set; }
        public int TotalConnectionsReceived { get; set; }
        public int TotalCommandsProcessed { get; set; }
        public long InstantaneousOpsPerSec { get; set; }
        public long TotalNetInputBytes { get; set; }
        public long TotalNetOutputBytes { get; set; }
        public string InstantaneousInputKbps { get; set; }
        public string InstantaneousOutputKbps { get; set; }
        public long RejectedConnections { get; set; }
        public long SyncFull { get; set; }
        public long SyncPartialOk { get; set; }
        public long SyncPartialErr { get; set; }
        public long ExpiredKeys { get; set; }
        public string ExpiredStalePerc { get; set; }
        public long ExpiredTimeCapReachedCount { get; set; }
        public long EvictedKeys { get; set; }
        public long KeyspaceHits { get; set; }
        public long KeyspaceMisses { get; set; }
        public long PubsubChannels { get; set; }
        public long PubsubPatterns { get; set; }
        public long LatestForkUsec { get; set; }
        public long MigrateCachedSockets { get; set; }
        public long SlaveExpiresTrackedKeys { get; set; }
        public long ActiveDefragHits { get; set; }
        public long ActiveDefragMisses { get; set; }
        public long ActiveDefragKeyHits { get; set; }
        public long ActiveDefragKeyMisses { get; set; }
        public string Role { get; set; }
        public long ConnectedSlaves { get; set; }
        public string MasterReplid { get; set; }
        public string MasterReplid2 { get; set; }
        public long MasterReplOffset { get; set; }
        public long SecondReplOffset { get; set; }
        public long ReplBacklogActive { get; set; }
        public long ReplBacklogSize { get; set; }
        public long ReplBacklogFirstByteOffset { get; set; }
        public long ReplBacklogHistlen { get; set; }
        public string UsedCpuSys { get; set; }
        public string UsedCpuSysChildren { get; set; }
        public string UsedCpuUser { get; set; }
        public string UsedCpuUserChildren { get; set; }
        public string ClusterEnabled { get; set; }
        public Dictionary<int, RedisDatabaseInfo> Databases { get; set; }
    }

    public class RedisDatabaseInfo
    {
        public int Keys { get; set; }
        public int Expires { get; set; }
        public long AvgTtl { get; set; }
    }
}
