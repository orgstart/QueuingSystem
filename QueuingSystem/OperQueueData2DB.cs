using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueuingSystem
{
    #region 定义委托 便于异步线程中使用
    public delegate int delUpdateDBbyAPI(string QUEUE_NUM, int state); //更新中间库排队表
    public delegate int delUpdateWJWYYDBbyAPI(string XH, int state);//更新预约信息
    public delegate int delAddDBbyAPI(string json);//添加新的排队信息到中间库
    public delegate string delGetYYNum(string cardNum, string departCode);//验证是否存在排队信息
    #endregion
    public class OperQueueData2DB
    {
        private string BBDM = "";//部门代码
        private string PDJUrl = ConfigurationManager.ConnectionStrings["PDJUrl"].ConnectionString;//更新中间库的接口地址 排队预约数据库
        private string LHYUrl = ConfigurationManager.ConnectionStrings["LHYUrl"].ConnectionString;//六合一数据接口地址
        private string YWUrl = ConfigurationManager.ConnectionStrings["YWUrl"].ConnectionString;//业务数据接口地址
        private string UPLOADUrl = ConfigurationManager.ConnectionStrings["UPLOADUrl"].ConnectionString;//上传图片服务路径
        public OperQueueData2DB(string _BBDM)
        {
            this.BBDM = _BBDM;
        }
        #region 通过本地简单三层更新本地数据库数据状态
        /// <summary>
        /// 通过sql语句更新本地库排队数据
        /// </summary>
        /// <returns></returns>
        public int UpdateDBbySql(string QUEUE_NUM, int state, string call_addr)
        {
            return new QueueSys.BLL.T_SYS_QUEUE().UpdateState(QUEUE_NUM, state, call_addr);
        }
        /// <summary>
        /// 把当前窗口为1的标记为0，即，当前窗口正在办理状态的标记为可再次叫号
        /// </summary>
        /// <param name="QUEUE_NUM"></param>
        /// <param name="state"></param>
        /// <param name="call_addr"></param>
        /// <returns></returns>
        public int UpdateDB_call(string QUEUE_NUM, int state, string call_addr)
        {
            return new QueueSys.BLL.T_SYS_QUEUE().UpdateState_call(QUEUE_NUM, state, call_addr);
        }

        public int UpdateStateYXC(string strQueue, int state)
        {
            return new QueueSys.BLL.T_SYS_QUEUE().UpdateStateYXC(strQueue, state);
        }

        public int UpdateCallNumXSL(string callnum, string queuenum)
        {

            return new QueueSys.BLL.T_SYS_QUEUE().UpdateCallNumXSL(callnum, queuenum);
        }

        public int UpdateStateXSL(string callnum, int state)
        {
            return new QueueSys.BLL.T_SYS_QUEUE().UpdateStateXSL(callnum, state);

        }
        #endregion

        #region 通过webAPI更新中间库状态

        #region 排队预约库

        public int InsertQueueByWebapi()
        {

            return 0;
        }


        /// <summary>
        /// 通过API 更新中间库数据
        /// </summary>
        /// <param name="QUEUE_NUM">排队号码</param>
        /// <param name="state">数据状态</param>
        /// <returns></returns>
        public int UpdateDBbyAPI(string QUEUE_NUM, int state)
        {
            HttpClient.sdnHttpWebRequest httpclient = new HttpClient.sdnHttpWebRequest();
            string strUrl = string.Format(PDJUrl + "/api/yypd/UpdateYYState?DEPART_CODE={0}&STATE={1}&NUM={2}", BBDM, state.ToString(), QUEUE_NUM);
            string reslut = httpclient.DoGet(strUrl);
            return 0;
        }

        /// <summary>
        /// 通过API 更新中间库数据
        /// </summary>
        /// <param name="QUEUE_NUM">排队号码</param>
        /// <param name="state">数据状态</param>
        /// <returns></returns>
        public int UpdateWJWYYDBbyAPI(string XH, int state)
        {
            HttpClient.sdnHttpWebRequest httpclient = new HttpClient.sdnHttpWebRequest();
            string strUrl = string.Format(PDJUrl + "/api/yypd/UpdateWJWYYState?XH={0}&STATE={1}", XH, state.ToString());
            string reslut = httpclient.DoGet(strUrl);

            return 0;
        }

        /// <summary>
        /// 通过API 插入数据到中间库数据
        /// </summary>
        /// <param name="QUEUE_NUM">排队号码</param>
        /// <param name="state">数据状态</param>
        /// <returns></returns>
        public int AddDBbyAPI(string json)
        {

            HttpClient.sdnHttpWebRequest httpclient = new HttpClient.sdnHttpWebRequest();
            string strUrl = string.Format(PDJUrl + "/api/yypd/AddYYNum?json={0}", json);
            string reslut = httpclient.DoGet(strUrl);
            return 0;
        }

        /// <summary>
        /// 通过API 插入数据到中间库数据
        /// </summary>
        /// <param name="QUEUE_NUM">排队号码</param>
        /// <param name="state">数据状态</param>
        /// <returns></returns>
        public string GetYYNum(string cardNum, string departCode)
        {

            HttpClient.sdnHttpWebRequest httpclient = new HttpClient.sdnHttpWebRequest();
            string serviceAddress = PDJUrl + "/api/yypd/GetYYNum?cardNum=" + cardNum + "&departCode=" + departCode;
            string s = httpclient.DoGet(serviceAddress);
            return s;
        }

        #endregion

        #region 业务库

        /// <summary>
        /// 根据IP和排队序列号
        /// </summary>
        /// <param name="strIP"></param>
        /// <param name="strSerialNum"></param>
        public void addCardMsg2QueueData(string strIP, string strSerialNum)
        {
            try
            {
                DataSet ds = new QueueSys.BLL.T_CARD_MSG().GetListBySerial(strIP, strSerialNum); //得到身份证信息数据
                if (ds != null)
                {
                    DataTable dt = ds.Tables[0];//得到对应的table
                    foreach (DataRow dr in dt.Rows)
                    {
                        HttpClient.sdnHttpWebRequest httpclient = new HttpClient.sdnHttpWebRequest();
                        string strUrl = YWUrl + "/api/PdjhInterface/AddRecCardMsg_New";
                        string[] arrParam = {
                                            dr["NAME"].ToString(),
                                            dr["CARD_NUM"].ToString(),
                                            dr["SEX"].ToString(),
                                            dr["NATION"].ToString(),
                                           string.IsNullOrWhiteSpace( dr["YEAR"].ToString())?"0": dr["YEAR"].ToString(),
                                             string.IsNullOrWhiteSpace( dr["MONTH"].ToString())?"0": dr["MONTH"].ToString(),
                                            string.IsNullOrWhiteSpace( dr["DAY"].ToString())?"0": dr["DAY"].ToString(),
                                            dr["ADDRESS"].ToString(),
                                            dr["SIGN"].ToString(),
                                            dr["STARTTIME"].ToString(),
                                            dr["ENDTIME"].ToString(),
                                           // dr["PHOTOPATH"].ToString(),
                                           dr["SERIALNUM"].ToString(),
                                           strIP
                                        };
                        string strContent = string.Format("{{\"NAME\":\"{0}\",\"CARDNOIDENTITYNUM\":\"{1}\",\"SEX\":\"{2}\",\"NATION\":\"{3}\",\"YEAR\":{4},\"MONTH\":{5},\"DAY\":{6},\"ADDRESS\":\"{7}\",\"SIGN\":\"{8}\",\"STARTTIME\":\"{9}\",\"ENDTIME\":\"{10}\",\"PHOTOPATH\":\"\",\"QHXXXLH\":\"{11}\",\"winip\":\"{12}\"}}", arrParam);
                        string reslut = httpclient.sdnDoPost(strUrl, strContent);
                        if (!string.IsNullOrEmpty(dr["PHOTOPATH"].ToString()) && File.Exists(dr["PHOTOPATH"].ToString()))
                        {
                            int iUploadRes = new sdnHttpUploadFIle().Upload_Request(UPLOADUrl, dr["PHOTOPATH"].ToString(), "3_" + dr["CARD_NUM"].ToString().Trim() + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".bmp");
                            if (iUploadRes == 1)
                            {
                                File.Delete(dr["PHOTOPATH"].ToString());
                            }
                        }


                    }
                }
            }
            catch (Exception ex)
            {
                Common.SysLog.WriteLog(ex, AppDomain.CurrentDomain.BaseDirectory);
            }
        }
        /// <summary>
        /// 录像开始
        /// </summary>
        /// <param name="strCardNo">身份证号码</param>
        /// <param name="strWinIp"></param>
        public void sdnStartRecVideo(string strCardNo, string strWinIp, string strqhsj)
        {
            HttpClient.sdnHttpWebRequest httpclient = new HttpClient.sdnHttpWebRequest();
            string strSJ = Convert.ToDateTime(strqhsj).ToString("yyyyMMddHHmmss");
            string strUrl = string.Format(YWUrl + "/api/RecVideoTemp/AddRecVideoTemp?filename={0}&identitynum={1}&strWinIp={2}", strCardNo + "_" + strSJ, strCardNo, strWinIp);
            string reslut = httpclient.DoGet(strUrl);

        }
        /// <summary>
        /// 录像结束
        /// </summary>
        /// <param name="strCardNo"></param>
        public void sdnEndRecVideo(string strCardNo, string strqhsj)
        {
            HttpClient.sdnHttpWebRequest httpclient = new HttpClient.sdnHttpWebRequest();
            string strSJ = Convert.ToDateTime(strqhsj).ToString("yyyyMMddHHmmss");
            string strUrl = string.Format(YWUrl + "/api/RecVideoTemp/EndRecVideoTemp?filename={0}", strCardNo + "_" + strSJ);
            string reslut = httpclient.DoGet(strUrl);
        }



        #endregion

        #region 六合一库
        /// <summary>
        /// 获取排队取票备案信息
        /// </summary>
        /// <param name="dwjgdm">单位机构代码（部门代码）</param>
        /// <param name="dwmc">单位名称</param>
        /// <param name="yhbs">警员标识</param>
        /// <param name="yhxm">警员姓名</param>
        /// <param name="sbkzjsjip">设备控制IP</param>
        /// <returns></returns>
        public DataTable getBAInfo(string dwjgdm, string dwmc, string yhbs, string yhxm, string sbkzjsjip, out string messsage)
        {
            //HttpClient.sdnHttpWebRequest httpclient = new HttpClient.sdnHttpWebRequest();
            //string serviceAddress = LHYUrl + "/api/DynamicInterface/GetBAXX?dwjgdm=" + dwjgdm + "&dwmc=" + dwmc + "&yhbs=" + yhbs + "&yhxm=" + yhxm + "&sbkzjsjip=" + sbkzjsjip;
            //string s = httpclient.DoGet(serviceAddress);
            try
            {
                string s = "";
                using (LHYserver.WebServiceSoapClient client = new LHYserver.WebServiceSoapClient())
                {
                    s = client.GetBAXX(dwjgdm, dwmc, yhbs, yhxm, sbkzjsjip);
                }
                DataTable dt = null;
                messsage = "";
                //  s = s.Replace(@"\n", "");
                Common.SysLog.WriteOptDisk(s, AppDomain.CurrentDomain.BaseDirectory); //记录获取接口日志
                if (!string.IsNullOrWhiteSpace(s))
                {
                    //string message = "";
                    dt = XmlHelper.Read_JSZHInfo(s, "queue", out messsage);
                }
                return dt;
            }
            catch (Exception ex)
            {
                messsage = ex.Message;
                Common.SysLog.WriteLog(ex, AppDomain.CurrentDomain.BaseDirectory);
                return null;
            }
        }
        /// <summary>
        /// 业务窗口发起叫号时，如果叫号评价系统中当时没有取号信息，可在后续生成新的取号信息时，调用该接口写入综合应用平台。对于代理人办理业务的，要提供申请人和代理人的信息
        /// </summary>
        /// <param name="dwjgdm">单位机关代码</param>
        /// <param name="dwmc">单位机关名称</param>
        /// <param name="yhbs">警员标识</param>
        /// <param name="yhxm">警员姓名</param>
        /// <param name="ywckjsjip">业务窗口计算机IP</param>
        /// <param name="sbkzjsjip">设备控制计算机IP</param>
        /// <param name="qhxxxlh">取号信息序列号</param>
        /// <param name="pdh">排队号</param>
        /// <param name="ywlb">业务类别</param>
        /// <param name="sfzmhm">身份证明号码</param>
        /// <param name="dlrsfzmhm">代理人身份证明号码（可空）</param>
        /// <param name="qhrxm">取号人姓名（可空）</param>
        /// <param name="qhsj">取号时间</param>
        /// <param name="rylb">人员类别</param>
        /// <returns></returns>
        public string writeBCQH(string dwjgdm, string dwmc, string yhbs, string yhxm, string ywckjsjip, string sbkzjsjip, string qhxxxlh, string pdh, string ywlb, string sfzmhm, string dlrsfzmhm, string qhrxm, string qhsj, string rylb, string jbr)
        {
            //HttpClient.sdnHttpWebRequest httpclient = new HttpClient.sdnHttpWebRequest();
            //string serviceAddress = LHYUrl + "/api/DynamicInterface/GetBAXX?dwjgdm=" + dwjgdm + "&dwmc=" + dwmc + "&yhbs=" + yhbs + "&yhxm=" + yhxm + "&ywckjsjip=" + ywckjsjip + "&sbkzjsjip=" + sbkzjsjip + "&qhxxxlh=" + qhxxxlh + "&pdh=" + pdh + "&ywlb=" + ywlb + "&sfzmhm=" + sfzmhm + "&dlrsfzmhm=" + dlrsfzmhm + "&qhrxm=" + qhrxm + "&qhsj=" + qhsj + "&rylb=" + rylb;
            //string s = httpclient.DoGet(serviceAddress);

            using (LHYserver.WebServiceSoapClient client = new LHYserver.WebServiceSoapClient())
            {
                return client.SetBXQH(dwjgdm, dwmc, yhbs, yhxm, ywckjsjip, sbkzjsjip, qhxxxlh, pdh, ywlb, sfzmhm, dlrsfzmhm, qhrxm, qhsj, rylb, jbr);
            }
        }

        /// <summary>
        /// 接收叫号评价系统写入的业务评价信息
        /// </summary>
        /// <param name="dwjgdm">单位机关代码</param>
        /// <param name="dwmc">单位机关名称</param>
        /// <param name="yhbs">警员标识</param>
        /// <param name="yhxm">警员姓名</param>
        /// <param name="qhxxxlh">取号信息序列号</param>
        /// <param name="pjlb">评价类别</param>
        /// <param name="pjjg">评价结果</param>
        /// <returns></returns>
        public string SetPJXX(string dwjgdm, string dwmc, string yhbs, string yhxm, string qhxxxlh, string pjlb, string pjjg)
        {
            //HttpClient.sdnHttpWebRequest httpclient = new HttpClient.sdnHttpWebRequest();
            //string serviceAddress = LHYUrl + "/api/DynamicInterface/GetBAXX?dwjgdm=" + dwjgdm + "&dwmc=" + dwmc + "&yhbs=" + yhbs + "&yhxm=" + yhxm + "&qhxxxlh=" + qhxxxlh + "&pjlb=" + pjlb + "&qhxxxlh=" + qhxxxlh + "&pjjg=" + pjjg;
            //string s = httpclient.DoGet(serviceAddress);
            //return s;
            using (LHYserver.WebServiceSoapClient client = new LHYserver.WebServiceSoapClient())
            {
                return client.SetPJXX(dwjgdm, dwmc, yhbs, yhxm, qhxxxlh, pjlb, pjjg);
            }
        }

        #endregion

        #endregion

        #region 公共方法

        /// <summary>
        /// Json 字符串 转换为 DataTable数据集合
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static DataTable ToDataTable(string json)
        {
            DataTable dataTable = new DataTable();  //实例化
            DataTable result;
            try
            {
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                javaScriptSerializer.MaxJsonLength = Int32.MaxValue; //取得最大数值
                ArrayList arrayList = javaScriptSerializer.Deserialize<ArrayList>(json);
                if (arrayList.Count > 0)
                {
                    foreach (Dictionary<string, object> dictionary in arrayList)
                    {
                        if (dictionary.Keys.Count<string>() == 0)
                        {
                            result = dataTable;
                            return result;
                        }
                        //Columns
                        if (dataTable.Columns.Count == 0)
                        {
                            foreach (string current in dictionary.Keys)
                            {
                                if (current != "data")
                                    dataTable.Columns.Add(current, dictionary[current].GetType());
                                else
                                {
                                    ArrayList list = dictionary[current] as ArrayList;
                                    foreach (Dictionary<string, object> dic in list)
                                    {
                                        foreach (string key in dic.Keys)
                                        {
                                            dataTable.Columns.Add(key, dic[key].GetType());
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                        //Rows
                        string root = "";
                        foreach (string current in dictionary.Keys)
                        {
                            if (current != "data")
                                root = current;
                            else
                            {
                                ArrayList list = dictionary[current] as ArrayList;
                                foreach (Dictionary<string, object> dic in list)
                                {
                                    DataRow dataRow = dataTable.NewRow();
                                    dataRow[root] = dictionary[root];
                                    foreach (string key in dic.Keys)
                                    {
                                        dataRow[key] = dic[key];
                                    }
                                    dataTable.Rows.Add(dataRow);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            result = dataTable;
            return result;
        }

        #endregion
    }
}
