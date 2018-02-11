
/// <summary>
/// Summary description for CommonUtil
/// </summary>
using ConstroSoft;
using System.Collections.Generic;
using NHibernate.Criterion;
using System.Linq.Expressions;
using System.Linq;
using NHibernate.Impl;
using System;
using System.Web.UI.WebControls;
using System.Globalization;
using System.Web;
using OfficeOpenXml;
using OfficeOpenXml.DataValidation;
using System.Web.Hosting;
using System.Web.UI;
using System.IO;
using System.Net;
using System.Text;
using RestSharp;
namespace ConstroSoft
{
    public class CommonUtil
    {
        private static readonly log4net.ILog log =
              log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public CommonUtil() { }
        public static string getRandomRefNo()
        {
            Random rd = new Random();
            return "" + rd.Next(100, 10000);
        }
        public static string getActivityRefNo(long Id, EnqActivityRecordType recordType)
        {
            string RefNo = (1000 + Id).ToString();
            if (recordType == EnqActivityRecordType.Activity) RefNo = "A" + RefNo;
            else if (recordType == EnqActivityRecordType.Action) RefNo = "S" + RefNo;
            else if (recordType == EnqActivityRecordType.Reminder) RefNo = "R" + RefNo;
            else if (recordType == EnqActivityRecordType.Note) RefNo = "N" + RefNo;
            else if (recordType == EnqActivityRecordType.Call) RefNo = "C" + RefNo;
            return RefNo;
        }
        public static string getLeadRefNo(long Id)
        {
            return "LD" + (1000 + Id).ToString();
        }
        public static string getEnquiryRefNo(long Id)
        {
            return "EQ" + (1000 + Id).ToString();
        }
        public static string getCustomerRefNo(long Id)
        {
            return "C" + (1000 + Id).ToString();
        }
        public static string getMasterTxRefNo(long Id)
        {
            return "" + (1000 + Id).ToString();
        }
        public static string getBookingRefNo(string propertyName, string towerName, long saleId)
        {
            return propertyName[0].ToString() + towerName[0].ToString() + (1000 + saleId).ToString();
        }
        public static string getAppendedNotyMsg(string msg1, string msg2)
        {
            if (string.IsNullOrWhiteSpace(msg1)) return msg2;
            else if (string.IsNullOrWhiteSpace(msg2)) return msg1;
            return msg1 + Constants.SEP1 + msg2;
        }
        public static string getNotySuccessMsg(string msg)
        {
            return (!string.IsNullOrWhiteSpace(msg)) ? Constants.NOTY_TYPE.SUCCESS + Constants.SEP2 + msg : "";
        }
        public static string getNotyErrorMsg(string msg)
        {
            return (!string.IsNullOrWhiteSpace(msg)) ? Constants.NOTY_TYPE.ERROR + Constants.SEP2 + msg : "";
        }
        public static string getNotyWarningMsg(string msg)
        {
            return (!string.IsNullOrWhiteSpace(msg)) ? Constants.NOTY_TYPE.WARNING + Constants.SEP2 + msg : "";
        }
        public static string getNotyInfoMsg(string msg)
        {
            return (!string.IsNullOrWhiteSpace(msg)) ? Constants.NOTY_TYPE.INFO + Constants.SEP2 + msg : "";
        }
        public static string getErrorMessage(Exception exp)
        {
            string message = Resources.Messages.system_error;
            if (exp is CustomException) message = exp.Message;
            return message;
        }
        public static void getCustomValidator(Page page, string message, string group)
        {
        	CustomValidator val = new CustomValidator();
            val.IsValid = false;
            val.ErrorMessage = message;
            val.ValidationGroup = group;
            page.Validators.Add(val);
        }
        public static bool hasEntitlement(UserDefinitionDTO userDefDto, params string[] entitlements)
        {
            bool result = false;
            if (userDefDto != null && userDefDto.UserRole.EntitlementAssigned != null)
            {
                foreach (string str in entitlements)
                {
                    result = userDefDto.UserRole.EntitlementAssigned.Contains(str);
                    if (result) break;
                }
            }
            return result;
        }
        public static PropertyProjection BuildProjection<T>(Expression<Func<object>> aliasExpression, Expression<Func<T, object>> propertyExpression)
        {
            string alias = ExpressionProcessor.FindMemberExpression(aliasExpression.Body);
            string property = ExpressionProcessor.FindMemberExpression(propertyExpression.Body);

            return Projections.Property(string.Format("{0}.{1}", alias, property));
        }
        public static string removeAppenders(string strTemp)
        {
            string result = "";
            if (!string.IsNullOrWhiteSpace(strTemp))
            {
                foreach(string appender in Constants.APPENDERS) {
                    strTemp = strTemp.Replace(appender, "");
                }
                result = strTemp.Trim();
            }
            return result;
        }
        public static decimal? getDecimalWithoutExt(string strVal)
        {
            return getDecimal(removeAppenders(strVal));
        }
        public static decimal? getDecimal(string strVal)
        {
            decimal? result = null;
            if (!string.IsNullOrWhiteSpace(strVal)) result = decimal.Parse(strVal);
            return result;
        }
        public static decimal getDecimaNotNulllWithoutExt(string strVal)
        {
            return getNotNullDecimal(removeAppenders(strVal));
        }
        public static decimal getNotNullDecimal(string strVal)
        {
            decimal result = decimal.Zero;
            if (!string.IsNullOrWhiteSpace(strVal)) result = decimal.Parse(strVal);
            return result;
        }
        public static string getAcntTransCommentPymtMethod(PaymentMethod pymtMethod, string mediaNo)
        {
            string tmpComment = "";
            if (pymtMethod == PaymentMethod.CASH)
                tmpComment = Constants.CASH_PYMT_MODE;
            else if (pymtMethod == PaymentMethod.CHEQUE)
                tmpComment = string.Format(Constants.CHQ_PYMT_MODE, mediaNo);
            else if (pymtMethod == PaymentMethod.DD)
                tmpComment = Constants.DD_PYMT_MODE;
            else if (pymtMethod == PaymentMethod.NEFT)
                tmpComment = Constants.NEFT_PYMT_MODE;
            else if (pymtMethod == PaymentMethod.RTGS)
                tmpComment = Constants.RTGS_PYMT_MODE;
            return tmpComment;
        }
        public static bool isGreaterThanToday(DateTime? date)
        {
            DateTime today = DateUtil.getUserLocalDateTime();
            return (date != null) ? date.Value.CompareTo(today) > 0 : false;
        }
        public static PropertyDTO getCurrentPropertyDTO(UserDefinitionDTO userDTO)
        {
            return (userDTO != null && userDTO.AssignedProperties != null) ? userDTO.AssignedProperties.Find(x => x.isUISelected) : null;
        }
        public static PropertyTowerDTO getStickyPrTowerDTO(UserDefinitionDTO userDTO)
        {
            PropertyDTO propertyDTO = getCurrentPropertyDTO(userDTO);
            if (userDTO.StickyTowerDTO == null || userDTO.StickyTowerDTO.Property.Id != propertyDTO.Id)
            {
                PropertyBO propertyBO = new PropertyBO();
                IList<PropertyTowerDTO> prTowerList = propertyBO.fetchPropertyTowerSelective(propertyDTO.Id);
                userDTO.StickyTowerDTO = prTowerList.ToList<PropertyTowerDTO>()[0];
            }
            return userDTO.StickyTowerDTO;
        }
        public static void setStickyPrTowerDTO(UserDefinitionDTO userDTO, long Id, string Name)
        {
            PropertyTowerDTO towerDTO = new PropertyTowerDTO();
            towerDTO.Id = Id;
            towerDTO.Name = Name;
            towerDTO.Property = getCurrentPropertyDTO(userDTO);
            userDTO.StickyTowerDTO = towerDTO;
        }
        public static void copyDropDownItems(DropDownList toDrp, DropDownList fromDrp) {
        	toDrp.Items.Clear();
            for (int i=0; i < fromDrp.Items.Count; i++) {
                toDrp.Items.Add(fromDrp.Items[i]);
            }
        }
        public static void copyDropDownItems(DropDownList toDrp, DropDownList fromDrp, bool excludeSelectItem)
        {
            toDrp.Items.Clear();
            for (int i = 0; i < fromDrp.Items.Count; i++)
            {
                if (!excludeSelectItem || !fromDrp.Items[i].Text.Equals(Constants.SELECT_ITEM[0]))
                {
                    toDrp.Items.Add(fromDrp.Items[i]);
                }
            }
        }
        public static  string addFilterToken(string filter, string token) {
        	return (string.IsNullOrWhiteSpace(filter)) ? token : filter + Constants.TOKEN_FIELD_DELIMITER + token;
        }
        public static string getRecordAddSuccessMsg(string recordName) {
        	return string.Format(Resources.Messages.RECORD_ADDED_DB_SUCCESS, recordName);
        }
        public static string getRecordModifySuccessMsg(string recordName) {
        	return string.Format(Resources.Messages.RECORD_MODIFY_DB_SUCCESS, recordName);
        }
        public static string getRecordDeleteSuccessMsg(string recordName) {
        	return string.Format(Resources.Messages.RECORD_DELETE_DB_SUCCESS, recordName);
        }
        public static string getRecordSoftDeleteSuccessMsg(string recordName) {
        	return string.Format(Resources.Messages.RECORD_DELETE_DB_SUCCESS, recordName);
        }
        public static void addListValidation(ExcelWorksheet worksheet, string cellNo, List<string> results)
        {
            var validation = worksheet.DataValidations.AddListValidation(cellNo);
            validation.ShowErrorMessage = true;
            validation.ErrorStyle = ExcelDataValidationWarningStyle.stop;
            validation.ErrorTitle = "Invalid value was entered";
            validation.Error = "Select a value from the list";
            foreach (string tmpStr in results)
            {
                validation.Formula.Values.Add(tmpStr);
            }
        }
        public static void addDateValidation(ExcelWorksheet worksheet, string cellNo)
        {
            using (ExcelRange col = ws.Cells[cellNo])
            {
                col.Style.Numberformat.Format =  "dd-MMM-yyyy";
                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            }
        }
        public static List<string> getDropdownItemNames(DropDownList drp) {
            List<string> list = new List<string>();
            foreach (ListItem li in drp.Items)
            {
                if (li.Text != "--Select--")
                    list.Add(li.Text);
            }
            return list;
        }
        public static List<string> getMasterDataNames(List<MasterControlDataDTO> results) {
            List<string> list = new List<string>();
            foreach (MasterControlDataDTO masterControlDataDTO in results)
            {
                list.Add(masterControlDataDTO.Name);
            }
            return list;
        }
        public static List<string> getEnumValues(Type enumType)
        {   
            if(!typeof(Enum).IsAssignableFrom(enumType))
                throw new ArgumentException("enumType should describe enum");
            Array names = Enum.GetNames(enumType);
            List<string> result = new List<string>(capacity:names.Length);
            for (int i = 0; i < names.Length; i++)
            {
                result.Add((string)names.GetValue(i));
            }

            return result;
        }
        public static List<string> getStepsToValidate(string startStep, string endStep) {
            List<string> steps = new List<string>();
            if(startStep.StartsWith("step") && endStep.StartsWith("step")) {
                long start = long.Parse(startStep.Replace("step", "").Trim());
                long end = long.Parse(endStep.Replace("step", "").Trim());
                do {
                    steps.Add("step" + start);
                    start++;
                } while(start < end);
            }
            return steps;
        }
        public static string appendCommaIfNot(string str) {
            string newStr = "";
            if (!string.IsNullOrWhiteSpace(str)) {
                newStr = (str.Trim().EndsWith(",")) ? str.Trim() : str.Trim()+",";
            }
            return newStr;
        }
        public static string appendBreakLine(string str) {
            return (!string.IsNullOrWhiteSpace(str)) ? str.Trim()+"<br/>" : "";
        }
        public static string getUserTempProfileImagePathRelative(string UserName)
        {
            return CommonUtil.appendNameToPath(Constants.DOCUMENT_MANAGEMENT.TMP_PATH, string.Format(Constants.DOCUMENT_MANAGEMENT.USER_TMP_PROFILE_IMG, UserName));
        }
        public static string getUserTempProfileImagePath(string UserName)
        {
            return HostingEnvironment.MapPath(getUserTempProfileImagePathRelative(UserName));
        }
        public static string getUserProfilePathRelative(string UserName)
        {
            return string.Format(Constants.DOCUMENT_MANAGEMENT.USER_PROFILE_PATH, UserName);
        }
        public static string getUserProfileImagePathRelative(string UserName)
        {
            return CommonUtil.appendNameToPath(getUserProfilePathRelative(UserName), Constants.DOCUMENT_MANAGEMENT.USER_PROFILE_IMG);
        }
        public static string getUserProfileImagePath(string UserName)
        {
            return HostingEnvironment.MapPath(getUserProfileImagePathRelative(UserName));
        }
        public static string appendNameToPath(string srcPath, string name)
        {
            return (srcPath.EndsWith("\\")) ? srcPath + name : srcPath + "\\" + name;
        }
        public static string getVirtualPath(string srcPath)
        {
            return srcPath.Replace(HostingEnvironment.MapPath(Constants.DOCUMENT_MANAGEMENT.DOC_MANAGEMENT_PATH), Constants.DOCUMENT_MANAGEMENT.DOC_MANAGEMENT_PATH);
        }
        public static bool isSystemManagedFolder(string folderName)
        {
            bool result = false;
            result = folderName.Equals(Constants.DOCUMENT_MANAGEMENT.CUSTOMER_TEMP_FOLDER) || folderName.Equals(Constants.DOCUMENT_MANAGEMENT.DEMAND_LETTER_FOLDER);
            return result;
        }
        public static string getDirectoryPath(List<FilePathBreadCrumbDTO> pathList)
        {
            string result = "";
            foreach (FilePathBreadCrumbDTO tmpDTO in pathList)
            {
                result = (string.IsNullOrWhiteSpace(result)) ? tmpDTO.Name : result + "\\" + tmpDTO.Name;
            }
            return result;
        }
        public static EnquiryActivity getEnquiryActivityAction(long EnquiryId, EnqActivityType enqActivityType, DateTime dateLogged, UserDefinitionDTO userDefDTO, params string[] parameters)
        {
            EnquiryActivity action = createNewEnquiryActivity(EnquiryId, EnqActivityRecordType.Action, userDefDTO);
            action.LoggedBy = new FirmMember();
            action.LoggedBy.Id = userDefDTO.FirmMember.Id;
            action.DateLogged = dateLogged;
            action.ActivityType = enqActivityType;
            action.Status = EnqLeadActivityStatus.Completed;
            action.RefNo = CommonUtil.getRandomRefNo();
            string comments = "";
            if (enqActivityType == EnqActivityType.CREATE) 
            	comments = "New Enquiry is created.";
            else if (enqActivityType == EnqActivityType.ASSIGNMENT)
                comments = "Enquiry is assigned to {0}." + Constants.NOTIFICATIONS.MSG_SEPARATOR + Constants.NOTIFICATIONS.PARAM_BOLD + parameters[0];
            else if (enqActivityType == EnqActivityType.RE_ASSIGNMENT)
                comments = "Enquiry is reassigned to {0}." + Constants.NOTIFICATIONS.MSG_SEPARATOR + Constants.NOTIFICATIONS.PARAM_BOLD + parameters[0];
            else if (enqActivityType == EnqActivityType.CONVERTED)
                comments = "Lead #{0} is converted to Enquiry." + Constants.NOTIFICATIONS.MSG_SEPARATOR + Constants.NOTIFICATIONS.PARAM_BOLD + parameters[0];
            else if (enqActivityType == EnqActivityType.BOOKING_CANCELLED)
            	comments = "Enquiry Lost - Unit booking done against this enquiry is cancelled.";
            else if (enqActivityType == EnqActivityType.LOST)
            	comments = "Enquiry Lost - Enquiry is Closed.";
            else if (enqActivityType == EnqActivityType.RE_OPENED)
            	comments = "Enquiry is reopened.";
            else if (enqActivityType == EnqActivityType.WON)
            	comments = "Enquiry Won - Unit booking is placed against this enquiry. Booking Ref# {0}"
                                + Constants.NOTIFICATIONS.MSG_SEPARATOR + Constants.NOTIFICATIONS.PARAM_BOLD + parameters[0];

            action.SystemComments = comments;
            action.Comments = "";
            return action;
        }
        public static EnquiryActivity createNewEnquiryActivity(long EnquiryId, EnqActivityRecordType recordType, UserDefinitionDTO userDefDto)
        {
            EnquiryActivity activity = new EnquiryActivity();
            activity.RecordType = recordType;
            activity.EnquiryDetail = new EnquiryDetail();
            activity.EnquiryDetail.Id = EnquiryId;
            activity.FirmNumber = userDefDto.FirmNumber;
            activity.InsertUser = userDefDto.Username;
            activity.UpdateUser = userDefDto.Username;
            return activity;
        }
        public static LeadActivity getLeadActivityAction(long LeadId, EnqActivityType enqActivityType, DateTime dateLogged, UserDefinitionDTO userDefDTO, params string[] parameters)
        {
            LeadActivity action = createNewLeadActivity(LeadId, EnqActivityRecordType.Action, userDefDTO);
            action.LoggedBy = new FirmMember();
            action.LoggedBy.Id = userDefDTO.FirmMember.Id;
            action.DateLogged = dateLogged;
            action.ActivityType = enqActivityType;
            action.Status = EnqLeadActivityStatus.Completed;
            action.RefNo = CommonUtil.getRandomRefNo();
            string comments = "";
            if (enqActivityType == EnqActivityType.CREATE) {
            	if (parameters[0].Equals("FACEBOOK")) comments = "New Lead is created through Facebook.";
            	else comments = "New Lead is created.";
            }            	
            else if (enqActivityType == EnqActivityType.ASSIGNMENT) 
            	comments = "Lead is assigned to {0}." + Constants.NOTIFICATIONS.MSG_SEPARATOR + Constants.NOTIFICATIONS.PARAM_BOLD + parameters[0];
            else if (enqActivityType == EnqActivityType.RE_ASSIGNMENT)
                comments = "Lead is reassigned to {0}." + Constants.NOTIFICATIONS.MSG_SEPARATOR + Constants.NOTIFICATIONS.PARAM_BOLD + parameters[0];
            else if (enqActivityType == EnqActivityType.CONVERTED)
                comments = "Lead is converted to Enquiry #{0}." + Constants.NOTIFICATIONS.MSG_SEPARATOR + Constants.NOTIFICATIONS.PARAM_BOLD + parameters[0];
            else if (enqActivityType == EnqActivityType.LOST)
            	comments = "Lead Lost - Lead is Closed.";
            else if (enqActivityType == EnqActivityType.FACEBOOK_ENQUIRY)
            	comments = "User has made enquiry through Facebook.";
            
            action.SystemComments = comments;
            action.Comments = "";
            return action;
        }
        public static LeadActivity createNewLeadActivity(long LeadId, EnqActivityRecordType recordType, UserDefinitionDTO userDefDto)
        {
            LeadActivity activity = new LeadActivity();
            activity.RecordType = recordType;
            activity.LeadDetail = new LeadDetail();
            activity.LeadDetail.Id = LeadId;
            activity.FirmNumber = userDefDto.FirmNumber;
            activity.InsertUser = userDefDto.Username;
            activity.UpdateUser = userDefDto.Username;
            return activity;
        }
        public static EnquiryActivityDTO createNewEnquiryActivityDTO(long EnquiryId, EnqActivityRecordType recordType, UserDefinitionDTO userDefDto)
        {
            EnquiryActivityDTO activityDTO = new EnquiryActivityDTO();
            activityDTO.RecordType = recordType;
            activityDTO.EnquiryDetail = new EnquiryDetailDTO();
            activityDTO.EnquiryDetail.Id = EnquiryId;
            activityDTO.FirmNumber = userDefDto.FirmNumber;
            activityDTO.InsertUser = userDefDto.Username;
            activityDTO.UpdateUser = userDefDto.Username;
            return activityDTO;
        }
        public static EnquiryActivityDTO createNewCallEnquiryActivityDTO(long EnquiryId, EnqActivityRecordType recordType, UserDefinitionDTO userDefDto,
            CallHistoryDTO callHistoryDTO)
        {
            EnquiryActivityDTO activityDTO = createNewEnquiryActivityDTO(EnquiryId, recordType, userDefDto);
            activityDTO.ReminderMode = ReminderMode.None;
            activityDTO.DateLogged = callHistoryDTO.StartTime;
            activityDTO.LoggedBy = userDefDto.FirmMember;
            activityDTO.ActivityType = EnqActivityType.CALL;
            activityDTO.CommunicationMedia = null;
            activityDTO.Comments = "";
            activityDTO.Status = EnqLeadActivityStatus.Completed;
            activityDTO.CallHistoryDTO = new CallHistoryDTO();
            activityDTO.CallHistoryDTO.Id = callHistoryDTO.Id;
            return activityDTO;
        }
        public static LeadActivityDTO createNewLeadActivityDTO(long LeadId, EnqActivityRecordType recordType, UserDefinitionDTO userDefDto)
        {
            LeadActivityDTO activityDTO = new LeadActivityDTO();
            activityDTO.RecordType = recordType;
            activityDTO.LeadDetail = new LeadDetailDTO();
            activityDTO.LeadDetail.Id = LeadId;
            activityDTO.FirmNumber = userDefDto.FirmNumber;
            activityDTO.InsertUser = userDefDto.Username;
            activityDTO.UpdateUser = userDefDto.Username;
            return activityDTO;
        }
        public static LeadActivityDTO createNewCallLeadActivityDTO(long LeadId, EnqActivityRecordType recordType, UserDefinitionDTO userDefDto,
            CallHistoryDTO callHistoryDTO)
        {
            LeadActivityDTO activityDTO = createNewLeadActivityDTO(LeadId, recordType, userDefDto);
            activityDTO.ReminderMode = ReminderMode.None;
            activityDTO.DateLogged = callHistoryDTO.StartTime;
            activityDTO.LoggedBy = userDefDto.FirmMember;
            activityDTO.ActivityType = EnqActivityType.CALL;
            activityDTO.CommunicationMedia = null;
            activityDTO.Comments = "";
            activityDTO.Status = EnqLeadActivityStatus.Completed;
            activityDTO.CallHistoryDTO = new CallHistoryDTO();
            activityDTO.CallHistoryDTO.Id = callHistoryDTO.Id;
            return activityDTO;
        }
        public static string redirectToDefaultPage(UserDefinitionDTO userDefDto, System.Web.SessionState.HttpSessionState Session) {
        	string redirectPage = Constants.URL.DEFAULT_HOME_PAGE;
        	string msg = "You do not have access to any of the Property. Please contact your supervisor.";
            List<String> UserEntitlements = userDefDto.UserRole.EntitlementAssigned;
        	bool hasAddPropertyEntitlement = UserEntitlements.Any(x => x == Constants.Entitlement.PROPERTY_ADD);
        	if(hasAddPropertyEntitlement) {
                PropertyDetailNavDTO navDTO = new PropertyDetailNavDTO();
                navDTO.Mode = PageMode.ADD;
                Session[Constants.Session.NAV_DTO] = navDTO;
        		redirectPage = Constants.URL.PROPERTY_DETAILS;
        		msg = "Please add new Property.";
        	}
        	Session.Add(Constants.Session.NOTY_MSG, CommonUtil.getNotyErrorMsg(msg));
        	return redirectPage;
        }
        public static byte[] downloadRecordingFile(string url) {
            byte[] fileBytes = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(url))
                {
                    var client = new RestClient(url);
                    var request = new RestRequest(Method.GET);
                    IRestResponse response = client.Execute(request);
                    if (response.StatusCode.GetDescription().Equals("OK"))
                    {
                        fileBytes = response.RawBytes;
                    } else {
                    	log.Error("recording url Status:" + response.StatusCode.GetDescription());
                    }
                }
            }
            catch (Exception e) { 
            	log.Error("recording url Status:", e);
            }
            return fileBytes;
        }
        public static string getFullAddress(AddressDTO addressDto)
        {
            return CommonUtil.appendCommaIfNot(addressDto.AddressLine1) + " " 
                    +CommonUtil.appendCommaIfNot(addressDto.AddressLine2) + " "
                    + CommonUtil.appendCommaIfNot(addressDto.Town) + " " + CommonUtil.appendCommaIfNot(addressDto.City.Name) + " " 
                    + CommonUtil.appendCommaIfNot(addressDto.State.Name) + " " + addressDto.Country.Name + " - " + addressDto.Pin;
        }
        public static VwCustomerEntityType getVwCustomerType(string EntityType)
        {
            if (EntityType.Equals("LD")) return VwCustomerEntityType.Lead;
            else if (EntityType.Equals("EQ")) return VwCustomerEntityType.Enquiry;
            else if (EntityType.Equals("UO")) return VwCustomerEntityType.UnitOwner;
            else return VwCustomerEntityType.UnitCoOwner;
        }
        public static CallStatus getCallStatus(string strCallStatus)
        {
            strCallStatus = strCallStatus.Replace("-", "");
            if (strCallStatus.Equals("queued")) return CallStatus.Queued;
            else if (strCallStatus.Equals("inprogress")) return CallStatus.Inprogress;
            else if (strCallStatus.Equals("failed")) return CallStatus.Failed;
            else if (strCallStatus.Equals("busy")) return CallStatus.Busy;
            else if (strCallStatus.Equals("noanswer")) return CallStatus.Noanswer;
            else return CallStatus.Completed;
        }
        public static string getContactFromExotel(string contact)
        {
            return (!string.IsNullOrWhiteSpace(contact)) ? contact.TrimStart('0') : "";
        }
        public static string validateContact(string contact)
        {
        	string msg = "";
        	if(!string.IsNullOrWhiteSpace(contact) && contact.Trim().Length < 10) {
        		msg = "Please enter valid Contact number.";
        	}
            return msg;
        }
        public static string getActualContact(string contact)
        {
        	string tmpContact = "";
        	if(!string.IsNullOrWhiteSpace(contact)) {
        		tmpContact = contact.Trim().Substring(Math.Max(0, contact.Length - 10));
        	}
            return tmpContact;
        }
        public static string getFileNameWithoutExtension(string fileName, string defaultName, string extension)
        {
        	string tmpFileName = defaultName;
        	if(!string.IsNullOrWhiteSpace(fileName)) {
        		int index = fileName.IndexOf(".", StringComparison.Ordinal);
        		tmpFileName = fileName.Trim().Substring(0, (index > 0) ? index : fileName.Trim().Length) + "." + extension;
        	}
            return tmpFileName;
        }
        public static JobHistoryDTO populateJobHistoryAddDTO(JobDTO jobDTO, string message)
        {
        	JobHistoryDTO jobHistoryDTO = new JobHistoryDTO();
            jobHistoryDTO.StartTime = DateUtil.getUserLocalDateTime();
            jobHistoryDTO.EndTime = DateUtil.getUserLocalDateTime();
            jobHistoryDTO.JobExecutionStatus = JobExecutionStatus.Failed;
            jobHistoryDTO.Job = jobDTO;
            jobHistoryDTO.FirmNumber = jobDTO.FirmNumber;
            jobHistoryDTO.Message = message;
            jobHistoryDTO.InsertUser = jobDTO.InsertUser;
            jobHistoryDTO.UpdateUser = jobDTO.UpdateUser;
            jobHistoryDTO.InsertDate = DateUtil.getUserLocalDateTime();
            jobHistoryDTO.UpdateDate = DateUtil.getUserLocalDateTime();
            return jobHistoryDTO;
        }
        public static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}