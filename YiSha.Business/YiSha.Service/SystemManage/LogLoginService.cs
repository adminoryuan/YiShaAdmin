﻿using System.Data.Common;
using System.Text;
using YiSha.DataBase;
using YiSha.DataBase.Extension;
using YiSha.Entity.SystemManage;
using YiSha.Model.Param.SystemManage;
using YiSha.Util;
using YiSha.Util.Model;

namespace YiSha.Service.SystemManage
{
    public class LogLoginService : Repository
    {
        #region 获取数据
        public async Task<List<LogLoginEntity>> GetList(LogLoginListParam param)
        {
            var strSql = new StringBuilder();
            List<DbParameter> filter = ListFilter(param, strSql);
            var list = await this.FindList<LogLoginEntity>(strSql.ToString(), filter.ToArray());
            return list.ToList();
        }

        public async Task<List<LogLoginEntity>> GetPageList(LogLoginListParam param, Pagination pagination)
        {
            var strSql = new StringBuilder();
            List<DbParameter> filter = ListFilter(param, strSql);
            var list = await this.FindList<LogLoginEntity>(strSql.ToString(), filter.ToArray(), pagination);
            return list.ToList();
        }

        public async Task<LogLoginEntity> GetEntity(long id)
        {
            return await this.FindEntity<LogLoginEntity>(id);
        }
        #endregion

        #region 提交数据
        public async Task SaveForm(LogLoginEntity entity)
        {
            if (entity.Id.IsNullOrZero())
            {
                await entity.Create();
                await this.Insert(entity);
            }
            else
            {
                await this.Update(entity);
            }
        }

        public async Task DeleteForm(string ids)
        {
            long[] idArr = TextHelper.SplitToArray<long>(ids, ',');
            await this.Delete<LogLoginEntity>(idArr);
        }

        public async Task RemoveAllForm()
        {
            await this.ExecuteBySql("truncate table SysLogLogin");
        }
        #endregion

        #region 私有方法
        private List<DbParameter> ListFilter(LogLoginListParam param, StringBuilder strSql)
        {
            strSql.Append(@"SELECT  a.Id,
                                    a.BaseCreateTime,
                                    a.BaseCreatorId,
                                    a.LogStatus,
                                    a.IpAddress,
                                    a.IpLocation,
                                    a.Browser,
                                    a.OS,
                                    a.Remark,
                                    b.UserName
                            FROM    SysLogLogin a
                                    LEFT JOIN SysUser b ON a.BaseCreatorId = b.Id
                            WHERE   1 = 1");
            var parameter = new List<DbParameter>();
            if (param != null)
            {
                if (!string.IsNullOrEmpty(param.UserName))
                {
                    strSql.Append(" AND b.UserName like @UserName");
                    parameter.Add(DbParameterExtension.CreateDbParameter("@UserName", '%' + param.UserName + '%'));
                }
                if (param.LogStatus > -1)
                {
                    strSql.Append(" AND a.LogStatus = @LogStatus");
                    parameter.Add(DbParameterExtension.CreateDbParameter("@LogStatus", param.LogStatus));
                }
                if (!string.IsNullOrEmpty(param.IpAddress))
                {
                    strSql.Append(" AND a.IpAddress like @IpAddress");
                    parameter.Add(DbParameterExtension.CreateDbParameter("@IpAddress", '%' + param.IpAddress + '%'));
                }
                if (!string.IsNullOrEmpty(param.StartTime.ToStr()))
                {
                    strSql.Append(" AND a.BaseCreateTime >= @StartTime");
                    parameter.Add(DbParameterExtension.CreateDbParameter("@StartTime", param.StartTime));
                }
                if (!string.IsNullOrEmpty(param.EndTime.ToStr()))
                {
                    param.EndTime = param.EndTime.Value.Date.Add(new TimeSpan(23, 59, 59));
                    strSql.Append(" AND a.BaseCreateTime <= @EndTime");
                    parameter.Add(DbParameterExtension.CreateDbParameter("@EndTime", param.EndTime));
                }
            }
            return parameter;
        }
        #endregion
    }
}
