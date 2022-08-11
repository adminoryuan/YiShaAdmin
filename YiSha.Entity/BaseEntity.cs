﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using YiSha.Entity.IDGenerator;

namespace YiSha.Entity
{
    /// <summary>
    /// 数据库实体的基类，所有的数据库实体属性类型都是可空值类型，为了在做条件查询的时候进行判断
    /// 虽然是可空值类型，null的属性值，在底层会根据属性类型赋值默认值，字符串是string?.empty，数值是0，日期是1970-01-01
    /// </summary>
    public class BaseEntity
    {
        /// <summary>
        /// 所有表的主键
        /// long返回到前端js的时候，会丢失精度，所以转成字符串
        /// </summary>
        [Key, Column("Id"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// WebApi没有Cookie和Session，所以需要传入Token来标识用户身份
        /// </summary>
        [NotMapped]
        public string Token { get; set; }

        /// <summary>
        /// 创建
        /// </summary>
        public virtual void Create()
        {
            this.Id = IDGeneratorHelper.Instance.GetId();
        }
    }

    /// <summary>
    /// 基础创建实体
    /// </summary>
    public class BaseCreateEntity : BaseEntity
    {
        /// <summary>
        /// 创建时间
        /// </summary>
        [Description("创建时间")]
        public DateTime BaseCreateTime { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        [Column("BaseCreatorId")]
        public long BaseCreatorId { get; set; }

        /// <summary>
        /// 创建
        /// </summary>
        public new async Task Create()
        {
            base.Create();

            if (this.BaseCreateTime == default)
            {
                this.BaseCreateTime = DateTime.Now;
            }

            if (this.BaseCreatorId == default)
            {
                var user = await Operator.Instance.Current(Token);
                this.BaseCreatorId = user != null ? user.UserId : 0;
            }
        }
    }

    /// <summary>
    /// 基础修改实体
    /// </summary>
    public class BaseModifyEntity : BaseCreateEntity
    {
        /// <summary>
        /// 数据更新版本，控制并发
        /// </summary>
        [Column("BaseVersion")]
        public int BaseVersion { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        [Column("BaseModifyTime"), Description("修改时间")]
        public DateTime BaseModifyTime { get; set; }

        /// <summary>
        /// 修改人ID
        /// </summary>
        [Column("BaseModifierId")]
        public long BaseModifierId { get; set; }

        /// <summary>
        /// 调整
        /// </summary>
        public async Task Modify()
        {
            this.BaseVersion = 0;
            this.BaseModifyTime = DateTime.Now;

            if (this.BaseModifierId == default)
            {
                var user = await Operator.Instance.Current();
                this.BaseModifierId = user != null ? user.UserId : 0;
            }
        }
    }

    /// <summary>
    /// 基本扩展实体
    /// </summary>
    public class BaseExtensionEntity : BaseModifyEntity
    {
        /// <summary>
        /// 是否删除 1是，0否
        /// </summary>
        [Column("BaseIsDelete"), JsonIgnore]
        public int BaseIsDelete { get; set; }

        /// <summary>
        /// 创建
        /// </summary>
        public new async Task Create()
        {
            this.BaseIsDelete = 0;

            await base.Create();

            await base.Modify();
        }

        /// <summary>
        /// 调整
        /// </summary>
        public new async Task Modify()
        {
            await base.Modify();
        }
    }

    /// <summary>
    /// 基础字段
    /// </summary>
    public class BaseField
    {
        /// <summary>
        /// 基础字段List
        /// </summary>
        public static string[] BaseFieldList { get; } = new string[]
        {
            "Id",
            "BaseIsDelete",
            "BaseCreateTime",
            "BaseModifyTime",
            "BaseCreatorId",
            "BaseModifierId",
            "BaseVersion",
        };
    }
}
