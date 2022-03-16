using System.ComponentModel.DataAnnotations;

namespace SmartCode.DocUtils.Dtos
{
    /// <summary>
    /// 数据库表字段dto
    /// </summary>
    public class ColumnDto
    {

        /// <summary>
        /// 序号
        /// </summary>
        [Display(Name = "序号")]
        public string ColumnOrder { get; set; }

        /// <summary>
        /// 列名
        /// </summary>
        [Display(Name = "列名")]
        public string ColumnName { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        [Display(Name = "数据类型")]
        public string ColumnTypeName { get; set; }

        /// <summary>
        /// 长度
        /// </summary>
        [Display(Name = "长度")]
        public string Length { get; set; }

        // <summary>
        // 小数位
        // </summary>
        //[Display(Name = "小数位数")]
        //public string Scale { get; set; }

        /// <summary>
        /// 主键
        /// </summary>
        [Display(Name = "主键")]
        public string IsPK { get; set; }

        /// <summary>
        /// 自增
        /// </summary>
        [Display(Name = "自增")]
        public string IsIdentity { get; set; }

        /// <summary>
        /// 允许空
        /// </summary>
        [Display(Name = "允许空")]
        public string CanNull { get; set; }

        /// <summary>
        /// 默认值
        /// </summary>
        [Display(Name = "默认值")]
        public string DefaultVal { get; set; }

        /// <summary>
        /// 注释
        /// </summary>
        [Display(Name = "列说明")]
        public string Comment { get; set; }

    }
}
