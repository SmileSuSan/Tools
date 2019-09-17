using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace K9
{
    public class SC
    {
        /// <summary>
        /// 设置数据源
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="type">类型</param>
        public static List<string> SetGridType(DataType type)
        {
            List<string> GridList = new List<string>();
            switch (type)
            {
                case DataType.TAB_SITE:
                    GridList.Add("网点编号;SITE_CODE");
                    GridList.Add("网点名称;SITE_NAME*");
                    break;
                case DataType.TAB_EMPLOYEE:
                    GridList.Add("员工编号;EMPLOYEE_CODE");
                    GridList.Add("员工姓名;EMPLOYEE_NAME*");
                    break;
                case DataType.TAB_DESTINATION:

                    GridList.Add("目的地;DESTINATION_NAME*");
                    GridList.Add("目的地编号;DESTINATION_CODE");
                    break;
                case DataType.TAB_CITY:
                    GridList.Add("城市名称;CITY_NAME*");
                    GridList.Add("省份名称;PROVINCE");
                    break;
                case DataType.TAB_PROVINCE:
                    GridList.Add("省份编号;PROVINCE_CODE");
                    GridList.Add("所属省份;PROVINCE*");
                    GridList.Add("所属片区;AREA_NAME");
                    break;
                case DataType.TAB_PROVINCE1:
                    GridList.Add("所属省份;PROVINCE*");
                    break;
                case DataType.TAB_AREA:
                    GridList.Add("省份名称;AREA_NAME*");
                    GridList.Add("国家名称;COUNTRY_NAME");
                    break;
                case DataType.TAB_COUNTY:
                    GridList.Add("区县名称;COUNTY_NAME*");
                    GridList.Add("城市名称;CITY_NAME");
                    GridList.Add("省份名称;PROVINCE");
                    break;
                case DataType.TAB_RANGE:
                    GridList.Add("区域名称;RANGE_NAME*");
                    GridList.Add("所属省份;PROVINCE");
                    GridList.Add("所属片区;AREA_NAME");
                    break;
                case DataType.TAB_CUSTOMER:
                    GridList.Add("客户编号;CUSTOMER_CODE");
                    GridList.Add("客户名称;CUSTOMER_NAME*");
                    break;
                case DataType.TAB_DISPCUSTOMER:
                    GridList.Add("客户名称;ACCEPT_MAN*");
                    GridList.Add("客户公司;ACCEPT_MAN_COMPANY");
                    GridList.Add("客户电话;ACCEPT_MAN_PHONE");
                    break;
                case DataType.TAB_PRO_KEEP_TYPE:
                    GridList.Add("仓库编号;TYPE_CODE");
                    GridList.Add("仓库类型;TYPE_NAME*");
                    break;
                case DataType.TAB_PROBLEM_TYPE:
                    GridList.Add("问题件编号;PROBLEM_CODE");
                    GridList.Add("问题件类型;PROBLEM*");
                    break;
                case DataType.TAB_BALANCE_TYPE:
                    GridList.Add("类型编号;BALANCE_CODE");
                    GridList.Add("结算类型;BALANCE_TYPE*");
                    break;
                case DataType.TAB_CAR_DATA:
                    GridList.Add("车牌号;TRUCK_NUM*");
                    break;
                case DataType.TAB_TAGBAG:
                    GridList.Add("所属包号;OWNER_PACKAGE");
                    GridList.Add("袋子编号;BAG_NO*");
                    break;
                case DataType.TAB_BANK_BELONG_CATEGORY:
                    GridList.Add("行属;BANK_BELONG*");
                    GridList.Add("行属类型;BANK_BELONG_TYPE");
                    GridList.Add("行属类别;BANK_BELONG_CATEGORY");
                    break;
                case DataType.TAB_EMPLOYEE1:
                    GridList.Add("线路编号;LINE_CODE*");
                    GridList.Add("员工编号;EMPLOYEE_CODE");
                    GridList.Add("员工姓名;EMPLOYEE_NAME");
                    break;
                case DataType.TAB_CUSTOMER1:
                    GridList.Add("项目编码;CUSTOMER_CODE*");
                    GridList.Add("客户名称;CUSTOMER_NAME");
                    GridList.Add("产品二级分类;PRODUCT_CLASS_TWO");
                    GridList.Add("产品三级分类;PRODUCT_CLASS_THREE");
                    GridList.Add("行属;BANK_BELONG");
                    GridList.Add("分公司;BRANCH_OFFICE");
                    GridList.Add("子公司;SUB_BRANCH_OFFICE");
                    break;
            }
            return GridList;
        }
    }

    public enum DataType
    {
        TAB_SITE,
        TAB_EMPLOYEE,
        TAB_DESTINATION,
        TAB_CITY,
        TAB_PROVINCE,
        TAB_PROVINCE1,
        TAB_AREA,
        TAB_COUNTY,
        TAB_RANGE,
        TAB_CUSTOMER,
        TAB_DISPCUSTOMER,
        TAB_PRO_KEEP_TYPE,
        TAB_PROBLEM_TYPE,
        TAB_BALANCE_TYPE,
        TAB_CAR_DATA,
        TAB_TAGBAG,
        TAB_BANK_BELONG_CATEGORY,
        TAB_EMPLOYEE1,
        TAB_CUSTOMER1
    }
}
