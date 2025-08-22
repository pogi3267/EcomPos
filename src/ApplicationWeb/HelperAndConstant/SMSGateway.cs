using System.Net;
using System.Xml;

namespace ApplicationWeb.HelperAndConstant
{
    public class SMSGateway
    {
        protected string ApiUrl = string.Empty; //"https://api.mobireach.com.bd";



        public static string Username = string.Empty; //"smart";
        public static string Password = string.Empty; // "sst@123@SST";
        public static string From = string.Empty; //"8801847121242";

        public string To { get; set; }
        public string Message { get; set; }


        public MobiReachResponse SendTextMessage(TextMessage yourTextMessage)
        {
            try
            {
                string method = "SendTextMessage";
                string url = @"" + ApiUrl + "/" + method + "?Username=" + Username + "&Password=" + Password + "&From=" + From + "&To=" + yourTextMessage.To + "&Message=" + yourTextMessage.Message + "";

                SMSGateway mobiReachMesseage = new SMSGateway();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.AutomaticDecompression = DecompressionMethods.GZip;
                HttpWebResponse responseMobiReach = (HttpWebResponse)request.GetResponse();
                Stream streamMobiReach = responseMobiReach.GetResponseStream();
                return mobiReachMesseage.MakeSingleResponse(streamMobiReach);

            }
            catch (Exception)
            {
                return new MobiReachResponse();
            }
        }

        public List<MobiReachResponse> SendTextMultiMessage(TextMessage yourTextMessage)
        {
            try
            {
                string method = "SendTextMultiMessage";
                string url = @"" + ApiUrl + "/" + method + "?Username=" + Username + "&Password=" + Password + "&From=" + From + "&To=" + yourTextMessage.To + "&Message=" + yourTextMessage.Message + "";

                SMSGateway mobiReachMesseage = new SMSGateway();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.AutomaticDecompression = DecompressionMethods.GZip;
                HttpWebResponse responseMobiReach = (HttpWebResponse)request.GetResponse();
                Stream streamMobiReach = responseMobiReach.GetResponseStream();
                return mobiReachMesseage.MakeMultiResponse(streamMobiReach);
            }
            catch (Exception)
            {
                return new List<MobiReachResponse>();
            }
        }
        public MobiReachResponse GetMessageStatus(string messeageId)
        {
            try
            {
                string method = "GetMessageStatus";
                string url = @"" + ApiUrl + "/" + method + "?Username=" + Username + "&Password=" + Password + "&MessageId=" + messeageId;

                SMSGateway mobiReachMesseage = new SMSGateway();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.AutomaticDecompression = DecompressionMethods.GZip;
                HttpWebResponse responseMobiReach = (HttpWebResponse)request.GetResponse();
                Stream streamMobiReach = responseMobiReach.GetResponseStream();
                return mobiReachMesseage.MakeSingleResponse(streamMobiReach);
            }
            catch (Exception)
            {
                return new MobiReachResponse();
            }
        }

        public MobiReachResponse MakeSingleResponse(Stream streamMobiReach)
        {
            MobiReachResponse response = new MobiReachResponse();

            List<string> resList = new List<string>();
            // Create an XmlReader
            using (XmlReader reader = XmlReader.Create(new StreamReader(streamMobiReach)))
            {
                XmlWriterSettings ws = new XmlWriterSettings();
                ws.Indent = true;
                {
                    int c = 0;
                    int t = -1;
                    int off = 3;
                    while (reader.Read())
                    {
                        c++;
                        switch (reader.NodeType)
                        {

                            case XmlNodeType.Element:
                                if (c > off)
                                {
                                    resList.Add("NA");
                                    t++;
                                }
                                break;
                            case XmlNodeType.Text:
                                if (c > off)
                                {
                                    resList[t] = reader.Value;
                                }
                                break;
                            case XmlNodeType.XmlDeclaration:
                            case XmlNodeType.ProcessingInstruction:
                                break;
                            case XmlNodeType.Comment:
                                break;
                            case XmlNodeType.EndElement:
                                break;
                        }
                    }

                }
            }
            if (resList.Count == 7)
            {
                response.MessageId = resList[0];
                response.Status = resList[1];
                response.StatusText = resList[2];
                response.ErrorCode = resList[3];
                response.ErrorText = resList[4];
                response.SMSCount = resList[5];
                response.CurrentCredit = resList[6];
                return response;
            }
            else
            {
                return new MobiReachResponse();
            }

        }

        public List<MobiReachResponse> MakeMultiResponse(Stream streamMobiReach)
        {
            List<MobiReachResponse> response = new List<MobiReachResponse>();

            List<string> resList = new List<string>();
            // Create an XmlReader
            using (XmlReader reader = XmlReader.Create(new StreamReader(streamMobiReach)))
            {
                XmlWriterSettings ws = new XmlWriterSettings();
                ws.Indent = true;
                {
                    int c = 0;
                    int t = -1;
                    int off = 3;
                    while (reader.Read())
                    {
                        c++;
                        switch (reader.NodeType)
                        {

                            case XmlNodeType.Element:
                                if (reader.Name == "ServiceClass")
                                {
                                    c = 3;
                                    t = -1;
                                }
                                if (c > off)
                                {
                                    resList.Add("NA");
                                    t++;
                                }
                                break;
                            case XmlNodeType.Text:
                                if (c > off)
                                {
                                    resList[t] = reader.Value;
                                }
                                break;
                            case XmlNodeType.XmlDeclaration:
                            case XmlNodeType.ProcessingInstruction:
                                break;
                            case XmlNodeType.Comment:
                                break;
                            case XmlNodeType.EndElement:
                                break;
                        }
                    }

                }
            }
            if (resList.Count % 7 == 0)
            {
                for (int i = 0; i < resList.Count / 7; i++)
                {
                    int p = i * 7;
                    MobiReachResponse r = new MobiReachResponse();
                    r.MessageId = resList[p + 0];
                    r.Status = resList[p + 1];
                    r.StatusText = resList[p + 2];
                    r.ErrorCode = resList[p + 3];
                    r.ErrorText = resList[p + 4];
                    r.SMSCount = resList[p + 5];
                    r.CurrentCredit = resList[p + 6];

                    response.Add(r);
                }
                return response;
            }
            else
            {
                return new List<MobiReachResponse>();
            }

        }

    }

    public class TextMessage
    {
        public string To { get; set; }
        public string Message { get; set; }
        public string MessageId { get; set; }
    }


    public class MobiReachResponse
    {

        public long Id { get; set; }

        public long SalesInvNo { get; set; }

        public string TrId { get; set; }

        public string MessageId { get; set; }

        public string Status { get; set; }

        public string StatusText { get; set; }

        public string ErrorCode { get; set; }

        public string ErrorText { get; set; }

        public string SMSCount { get; set; }

        public string CurrentCredit { get; set; }

        public DateTime EntryDateTime { get; set; }

        public string EntryIp { get; set; }

        public string RequestBy { get; set; }

        public long RequestById { get; set; }
    }

    public class TextMessageStatus
    {
        public string MessageId { get; set; }
    }

    public class MobiReachResponseMulti
    {
        public List<MobiReachResponse> MobiReachStatusResponses { get; set; }
        public MobiReachResponseMulti()
        {
            MobiReachStatusResponses = new List<MobiReachResponse>();
        }
    }
}
