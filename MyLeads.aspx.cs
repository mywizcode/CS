using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using ConstroSoft;
using System.Globalization;
using Vladsm.Web.UI.WebControls;
using ConstroSoft.Logic.BO;

public partial class MyLeads : System.Web.UI.Page
{
    private static readonly log4net.ILog log =
               log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    string commonError = "commonError";
    string addLeadModalError = "addLeadModalError";
    string addMasterDataError = "addMasterDataError";
    string SearchFilterModal = "SearchFilterModal";
    string addLeadModal = "addLeadModal";
    string addMasterDataModal = "addMasterDataModal";
    DropdownBO drpBO = new DropdownBO();
    EnquiryBO enquiryBO = new EnquiryBO();
    SoldPropertyUnitBO soldUnitBO = new SoldPropertyUnitBO();
    MasterDataBO masterDataBO = new MasterDataBO();
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (ApplicationUtil.isSessionActive(Session))
            {
                MyLeadsNavDTO navDto = ApplicationUtil.getPageNavDTO<MyLeadsNavDTO>(Session);
                if (!CommonUtil.hasEntitlement(getUserDefinitionDTO(), Constants.Entitlement.MENU_MY_LEADS)) Response.Redirect(Constants.URL.ACCESS_DENIED, true);
                if (CommonUtil.getCurrentPropertyDTO(getUserDefinitionDTO()) != null) doInit(navDto); else Response.Redirect(CommonUtil.redirectToDefaultPage(getUserDefinitionDTO(), Session), true);
            }
            else
            {
                Response.Redirect(Constants.URL.LOGIN, true);
            }
        }
    }
    /**
     * This method is called just before the page is rendered. So any change in state of the element is applied.
     **/
    protected void Page_PreRender(object sender, EventArgs e)
    {
        if (ApplicationUtil.isSessionActive(Session))
        {
        	if (ApplicationUtil.isSubPageRendered(Page))
            {
                (this.Master as CSMaster).setNotyMsg(ApplicationUtil.getSessionNotyMsg(Session));
                preRenderInitFormElements();
            }
            if (ApplicationUtil.isAsyncPostBack(Page)) initBootstrapComponantsFromServer();
        }
        else
        {
            Response.Redirect(Constants.URL.LOGIN, true);
        }
    }
    private void preRenderInitFormElements()
    {
        renderPageFieldsWithEntitlement();
        addCheckBoxAttributes();
    }
    private void renderPageFieldsWithEntitlement()
    {
    	lnkAddLeadBtn.Visible = CommonUtil.hasEntitlement(getUserDefinitionDTO(), Constants.Entitlement.LEAD_ADD);
        lnkAddLeadSource.Visible = CommonUtil.hasEntitlement(getUserDefinitionDTO(), Constants.Entitlement.MASTER_DATA_ADD);
    }
    public void initBootstrapComponantsFromServer()
    {
    	ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "BootStrapComponants", string.Format("initBootstrapComponants('{0}');", ApplicationUtil.getParentToApplyCSS(Page)), true);
    }
    private UserDefinitionDTO getUserDefinitionDTO()
    {
        return (UserDefinitionDTO)Session[Constants.Session.USERDEFINITION];
    }
    private void initDropdowns()
    {
        UserDefinitionDTO userDefDto = getUserDefinitionDTO();
        drpBO.drpDataBase(drpSourceFilter, DrpDataType.MASTER_CONTROL_DATA, MasterDataType.ENQUIRY_SOURCE.ToString(), Constants.SELECT_ITEM, userDefDto.FirmNumber);
        CommonUtil.copyDropDownItems(drpLeadSource, drpSourceFilter);
        drpBO.drpEnum<LeadStatus>(drpStatusFilter, Constants.SELECT_ITEM);
        drpBO.drpDataBase(drpLeadSalutation, DrpDataType.MASTER_CONTROL_DATA, MasterDataType.SALUTATION.ToString(), null, userDefDto.FirmNumber);

    }
    private void addCheckBoxAttributes()
    {
        cbShowAllLeads.InputAttributes.Add("class", "styled block-ui-change");
        cbShowAllLeads.InputAttributes.Add("data-panel", "blockui-panel-1");
    }
    public void setErrorMessage(string message, string group)
    {
    	string[] pageErrorGrp = { commonError };
        if (pageErrorGrp.Contains(group))
        {
            (this.Master as CSMaster).setNotyMsg(CommonUtil.getNotyErrorMsg(message));
            scrollToFieldHdn.Value = Constants.SCROLL_TOP;
        }
        else
        {
	        CustomValidator val = new CustomValidator();
	        val.IsValid = false;
	        val.ErrorMessage = message;
	        val.ValidationGroup = group;
	        this.Page.Validators.Add(val);
        }
    }
    private void doInit(MyLeadsNavDTO navDto)
    {
        Session[Constants.Session.PAGE_DATA] = new MyLeadsPageDTO();
        initDropdowns();
        LeadFilterDTO FilterDTO = new LeadFilterDTO();
        //Default Open Leads will be shown when page is loaded
        FilterDTO.Status = LeadStatus.Open;
        setSearchFilter(FilterDTO);
        initPageAfterRedirect(navDto);
    }
    private void initPageAfterRedirect(MyLeadsNavDTO navDto)
    {
        try
        {
            if (navDto != null)
            {
                setSearchFilter(navDto.filterDTO);
                cbShowAllLeads.Checked = navDto.AllUserLeads;
            }
            loadLeadSearchGrid();
        }
        catch (Exception exp)
        {
            log.Error(exp.Message, exp);
            throw exp;
        }
    }
    private MyLeadsPageDTO getSessionPageData()
    {
        return (MyLeadsPageDTO)Session[Constants.Session.PAGE_DATA];
    }
    private List<LeadDetailDTO> getSearchList()
    {
        return getSessionPageData().SearchResult;
    }
    private LeadDetailDTO getSearchDTO(long Id)
    {
        List<LeadDetailDTO> searchList = getSearchList();
        LeadDetailDTO selectedLeadDTO = null;
        if (searchList != null && searchList.Count > 0)
        {
        	selectedLeadDTO = searchList.Find(c => c.Id == Id);
        }
        return selectedLeadDTO;
    }
    private void populateLeadSearchGrid(List<LeadDetailDTO> tmpList)
    {
        myLeadsSearchGrid.DataSource = new List<LeadDetailDTO>();
        if (tmpList != null)
        {
            assignUiIndexToLeads(tmpList);
            myLeadsSearchGrid.DataSource = tmpList;
        }
        myLeadsSearchGrid.DataBind();
    }
    private void assignUiIndexToLeads(List<LeadDetailDTO> tmpList)
    {
        if (tmpList != null && tmpList.Count > 0)
        {
            long uiIndex = 1;
            foreach (LeadDetailDTO tmpDTO in tmpList)
            {
                tmpDTO.UiIndex = uiIndex++;
                tmpDTO.RowInfo = CommonUIConverter.getGridViewRowInfo(tmpDTO);
                tmpDTO.FullName = CommonUIConverter.getCustomerFullName(tmpDTO.Salutation.Name, tmpDTO.FirstName, tmpDTO.MiddleName, tmpDTO.LastName);
                tmpDTO.Assignee.FullName = CommonUIConverter.getCustomerFullName(tmpDTO.Assignee.FirstName, tmpDTO.Assignee.LastName);
                tmpDTO.NoOfDaysAssgined = (DateUtil.getUserLocalDate() - tmpDTO.AssignedDate.Value).Days;
            }
        }
    }
    protected bool isAllUserSelected()
    {
        return cbShowAllLeads.Checked;
    }
    private void loadLeadSearchGrid()
    {
        MyLeadsPageDTO PageDTO = getSessionPageData();
        UserDefinitionDTO userDefDTO = getUserDefinitionDTO();
        PropertyDTO property = CommonUtil.getCurrentPropertyDTO(userDefDTO);
        IList<LeadDetailDTO> results = enquiryBO.fetchMyLeadsGridData(property.Id, userDefDTO.FirmMember.Id, cbShowAllLeads.Checked, getSearchFilter());
        PageDTO.SearchResult = (results != null) ? results.ToList<LeadDetailDTO>() : new List<LeadDetailDTO>();
        populateLeadSearchGrid(PageDTO.SearchResult);
    }
    private MyLeadsNavDTO getCurrentPageNavigation()
    {
        MyLeadsPageDTO PageDTO = getSessionPageData();
        MyLeadsNavDTO navDTO = new MyLeadsNavDTO();
        navDTO.filterDTO = getSearchFilter();
        navDTO.AllUserLeads = cbShowAllLeads.Checked;
        return navDTO;
    }
    protected void onClickActiveHistoryBtn(object sender, EventArgs e)
    {
        try
        {
            LinkButton rd = (LinkButton)sender;
            long selectedId = long.Parse(rd.Attributes["data-pid"]);
            LeadActivityHistoryNavDTO navDTO = new LeadActivityHistoryNavDTO();
            navDTO.LeadId = selectedId;
            navDTO.PrevNavDto = getCurrentPageNavigation();
            Session[Constants.Session.NAV_DTO] = navDTO;
            Response.Redirect(Constants.URL.LEAD_ACTIVITY_HISTORY, true);
        }
        catch (Exception exp)
        {
            log.Error(exp.Message, exp);
            setErrorMessage(CommonUtil.getErrorMessage(exp), commonError);
        }
    }
    protected void onClickEnquiryDetailsBtn(object sender, EventArgs e)
    {
        try
        {
            LinkButton rd = (LinkButton)sender;
            long selectedId = long.Parse(rd.Attributes["data-pid"]);
            LeadDetailDTO selectedLeadDTO = getSearchDTO(selectedId);
            if(selectedLeadDTO.Status == LeadStatus.Converted && !string.IsNullOrWhiteSpace(selectedLeadDTO.EnquiryDetail.EnquiryRefNo)) {
            	EnquiryDetailNavDTO navDTO = new EnquiryDetailNavDTO();
                navDTO.Mode = PageMode.VIEW;
                navDTO.EnquiryId = selectedLeadDTO.EnquiryDetail.Id;
                navDTO.PrevNavDto = getCurrentPageNavigation();
                Session[Constants.Session.NAV_DTO] = navDTO;
                Response.Redirect(Constants.URL.ENQUIRY_DETAILS, true);
            } else {
                (this.Master as CSMaster).setNotyMsg(CommonUtil.getNotyErrorMsg(string.Format("Lead# {0} is is not Converted.", selectedLeadDTO.LeadRefNo)));
            }
        }
        catch (Exception exp)
        {
            log.Error(exp.Message, exp);
            setErrorMessage(CommonUtil.getErrorMessage(exp), commonError);
        }
    }
    protected void onChangeShowAllLeads(object sender, EventArgs e)
    {
        try
        {
            loadLeadSearchGrid();
        }
        catch (Exception exp)
        {
            log.Error(exp.Message, exp);
            setErrorMessage(CommonUtil.getErrorMessage(exp), commonError);
        }
    }
    //Filter Criteria - Enquiry Search - Start
    private LeadFilterDTO getSearchFilter()
    {
        return getSessionPageData().FilterDTO;
    }
    protected void onClickSearchFilter(object sender, EventArgs e)
    {
        try
        {
            activeModalHdn.Value = SearchFilterModal;
            LeadFilterDTO filterDTO = getSearchFilter();
            if (filterDTO.FirstName != null) txtFirstName.Text = filterDTO.FirstName; else txtFirstName.Text = null;
            if (filterDTO.LastName != null) txtLastName.Text = filterDTO.LastName; else txtLastName.Text = null;
            if (filterDTO.Contact != null) txtContact.Text = filterDTO.Contact; else txtContact.Text = null;
            if (filterDTO.LeadRefNo != null) txtLeadRefNo.Text = filterDTO.LeadRefNo; else txtLeadRefNo.Text = null;
            if (filterDTO.SourceId > 0) drpSourceFilter.Text = filterDTO.SourceId.ToString(); else drpSourceFilter.ClearSelection();
            if (filterDTO.Status != null) drpStatusFilter.Text = filterDTO.Status.ToString(); else drpStatusFilter.Text = null;
        }
        catch (Exception exp)
        {
            log.Error(exp.Message, exp);
            setErrorMessage(CommonUtil.getErrorMessage(exp), commonError);
        }
    }
    protected void applySearchFilterCriteria(object sender, EventArgs e)
    {
        try
        {
            LeadFilterDTO filterDTO = new LeadFilterDTO();
            if (!string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                filterDTO.FirstName = txtFirstName.Text;
            }
            if (!string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                filterDTO.LastName = txtLastName.Text;
            }
            if (!string.IsNullOrWhiteSpace(txtContact.Text))
            {
                filterDTO.Contact = txtContact.Text;
            }
            if (!string.IsNullOrWhiteSpace(txtLeadRefNo.Text))
            {
                filterDTO.LeadRefNo = txtLeadRefNo.Text;
            }
            if (!string.IsNullOrWhiteSpace(drpSourceFilter.Text))
            {
                filterDTO.SourceId = long.Parse(drpSourceFilter.Text);
                filterDTO.Source = drpSourceFilter.SelectedItem.Text;
            }
            if (!string.IsNullOrWhiteSpace(drpStatusFilter.Text))
            {
                filterDTO.Status = EnumHelper.ToEnum<LeadStatus>(drpStatusFilter.SelectedItem.Text);
            }
            setSearchFilter(filterDTO);
            loadLeadSearchGrid();
        }
        catch (Exception exp)
        {
            log.Error(exp.Message, exp);
            setErrorMessage(CommonUtil.getErrorMessage(exp), commonError);
        }
    }

    protected void clearSearchFilter(object sender, EventArgs e)
    {
        try
        {
            setSearchFilter(null);
            loadLeadSearchGrid();
        }
        catch (Exception exp)
        {
            log.Error(exp.Message, exp);
            setErrorMessage(CommonUtil.getErrorMessage(exp), commonError);
        }
    }
    private void setSearchFilter(LeadFilterDTO searchFilterDTO)
    {
        getSessionPageData().FilterDTO = (searchFilterDTO != null) ? searchFilterDTO : new LeadFilterDTO();
        setSearchFilterTokens();
    }
    protected void cancelSearchFilterModal(object sender, EventArgs e)
    {
        try
        {

        }
        catch (Exception exp)
        {
            log.Error(exp.Message, exp);
            setErrorMessage(CommonUtil.getErrorMessage(exp), commonError);
        }
    }
    protected void removeSearchFilterToken(object sender, EventArgs e)
    {
        try
        {
            string token = filterRemoveHdn.Value;
            filterRemoveHdn.Value = "";
            LeadFilterDTO filterDTO = getSearchFilter();
            if (token.StartsWith(Constants.FILTER.FIRST_NAME))
            {
                filterDTO.FirstName = null;
            }
            else if (token.StartsWith(Constants.FILTER.LAST_NAME))
            {
                filterDTO.LastName = null;
            }
            else if (token.StartsWith(Constants.FILTER.CONTACT))
            {
                filterDTO.Contact = null;
            }
            else if (token.StartsWith(Constants.FILTER.LEAD_REF_NO))
            {
                filterDTO.LeadRefNo = null;
            }
            else if (token.StartsWith(Constants.FILTER.SOURCE))
            {
                filterDTO.SourceId = 0;
                filterDTO.Source = null;
            }
            else if (token.StartsWith(Constants.FILTER.STATUS))
            {
                filterDTO.Status = null;
            }

            setSearchFilterTokens();
            loadLeadSearchGrid();
        }
        catch (Exception exp)
        {
            log.Error(exp.Message, exp);
            setErrorMessage(CommonUtil.getErrorMessage(exp), commonError);
        }
    }
    private void setSearchFilterTokens()
    {
        LeadFilterDTO filterDTO = getSearchFilter();
        string filter = null;
        if (filterDTO != null)
        {
            if (filterDTO.FirstName != null) filter = CommonUtil.addFilterToken(filter, Constants.FILTER.FIRST_NAME + filterDTO.FirstName);
            if (filterDTO.LastName != null) filter = CommonUtil.addFilterToken(filter, Constants.FILTER.LAST_NAME + filterDTO.LastName);
            if (filterDTO.Contact != null) filter = CommonUtil.addFilterToken(filter, Constants.FILTER.CONTACT + filterDTO.Contact);
            if (filterDTO.LeadRefNo != null) filter = CommonUtil.addFilterToken(filter, Constants.FILTER.LEAD_REF_NO + filterDTO.LeadRefNo);
            if (filterDTO.Source != null) filter = CommonUtil.addFilterToken(filter, Constants.FILTER.SOURCE + filterDTO.Source);
            if (filterDTO.Status != null) filter = CommonUtil.addFilterToken(filter, Constants.FILTER.STATUS + filterDTO.Status);
        }
        txtSelectedFilter.Text = filter;
    }
    //Filter Criteria - Enquiry Search - End
    //Master Data Modal save logic - Start
    protected void saveMasterData(object sender, EventArgs e)
    {
        try
        {
            String errorMsg = "";
            UserDefinitionDTO userDefDto = getUserDefinitionDTO();
            if (MasterDataType.ENQUIRY_SOURCE.ToString().Equals(masterDataModalTypeHdn.Value))
            {
                MasterControlDataDTO masterDataDto = CommonUIConverter.populateMasterDataDTOAdd(MasterDataType.ENQUIRY_SOURCE.ToString(), txtMasterDataInput1.Text,
                        txtMasterDataInput2.Text, userDefDto);
                errorMsg = validateMasterDataModalInput(masterDataDto, "Enquiry/Lead Source");
                if (string.IsNullOrWhiteSpace(errorMsg))
                {
                    masterDataBO.saveMasterData(masterDataDto);
                    drpBO.drpDataBase(drpSourceFilter, DrpDataType.MASTER_CONTROL_DATA, MasterDataType.ENQUIRY_SOURCE.ToString(), Constants.SELECT_ITEM, userDefDto.FirmNumber);
                    CommonUtil.copyDropDownItems(drpLeadSource, drpSourceFilter);
                }
            }

            if (!string.IsNullOrWhiteSpace(errorMsg))
            {
                activeModalHdn.Value = addMasterDataModal;
                setErrorMessage(errorMsg, addMasterDataError);
            }
            else
            {
                resetMasterDataModalFields();
                setParentModalFlag();
            }
        }
        catch (Exception exp)
        {
            log.Error(exp.Message, exp);
            activeModalHdn.Value = "";
            setErrorMessage(CommonUtil.getErrorMessage(exp), addMasterDataError);
        }
    }
    protected void cancelMasterDataModal(object sender, EventArgs e)
    {
        try
        {
            resetMasterDataModalFields();
            setParentModalFlag();
        }
        catch (Exception exp)
        {
            log.Error(exp.Message, exp);
            setErrorMessage(CommonUtil.getErrorMessage(exp), addMasterDataError);
        }
    }
    private void resetMasterDataModalFields()
    {
        txtMasterDataInput1.Text = "";
        txtMasterDataInput2.Text = "";
        masterDataModalTypeHdn.Value = "";
    }
    private string validateMasterDataModalInput(MasterControlDataDTO masterDataDto, string type)
    {
        string errorMsg = "";
        if (string.IsNullOrWhiteSpace(masterDataDto.Name))
        {
            errorMsg = string.Format(Resources.Messages.TEXTFIELD_REQUIRED, type);
        }
        else if (masterDataBO.isAlreadyExist(masterDataDto))
        {
            errorMsg = string.Format(Resources.Messages.ALREADY_EXIST_DB_ERROR, type);
        }
        return errorMsg;
    }
    private void setParentModalFlag()
    {
        activeModalHdn.Value = masterDataParentModalHdn.Value;
        masterDataParentModalHdn.Value = "";
    }
    //Master Data Modal save logic - End
    //Lead Modal - Start
    private void initLeadModalFields()
    {
    }
    private void initLeadSectionFields(LeadDetailDTO leadDetailDTO)
    {
    	if (leadDetailDTO != null && leadDetailDTO.Salutation != null) drpLeadSalutation.Text = leadDetailDTO.Salutation.Id.ToString(); else drpLeadSalutation.ClearSelection();
        if (leadDetailDTO != null) txtLeadFirstName.Text = leadDetailDTO.FirstName; else txtLeadFirstName.Text = null;
        if (leadDetailDTO != null) txtLeadMiddleName.Text = leadDetailDTO.MiddleName; else txtLeadMiddleName.Text = null;
        if (leadDetailDTO != null) txtLeadLastName.Text = leadDetailDTO.LastName; else txtLeadLastName.Text = null;
        if (leadDetailDTO != null) txtLeadDate.Text = DateUtil.getCSDate(leadDetailDTO.LeadDate); else txtLeadDate.Text = DateUtil.getDateTime(Constants.DATETIME_FORMAT_SHORT);
        if (leadDetailDTO != null && leadDetailDTO.ContactInfo != null) txtLeadContact.Text = leadDetailDTO.ContactInfo.Contact; else txtLeadContact.Text = null;
        if (leadDetailDTO != null && leadDetailDTO.ContactInfo != null) txtLeadAltContact.Text = leadDetailDTO.ContactInfo.AltContact; else txtLeadAltContact.Text = null;
        if (leadDetailDTO != null && leadDetailDTO.ContactInfo != null) txtLeadEmail.Text = leadDetailDTO.ContactInfo.Email; else txtLeadEmail.Text = null;
        if (leadDetailDTO != null && leadDetailDTO.Budget != null) txtLeadBudget.Text = leadDetailDTO.Budget.ToString(); else txtLeadBudget.Text = null;
        if (leadDetailDTO != null && leadDetailDTO.Source != null) drpLeadSource.Text = leadDetailDTO.Source.Id.ToString(); else drpLeadSource.ClearSelection();
    }
    private void populateAddressFromUI(LeadDetailDTO leadDetailDTO)
    {
        leadDetailDTO.Salutation = CommonUIConverter.getMasterControlDTO(drpLeadSalutation.Text, drpLeadSalutation.SelectedItem.Text);
        leadDetailDTO.FirstName = txtLeadFirstName.Text;
        leadDetailDTO.MiddleName = txtLeadMiddleName.Text;
        leadDetailDTO.LastName = txtLeadLastName.Text;
        leadDetailDTO.LeadDate = DateUtil.getCSDateTimeShort(txtLeadDate.Text).Value;
        leadDetailDTO.ContactInfo.Contact = txtLeadContact.Text;
        leadDetailDTO.ContactInfo.AltContact = txtLeadAltContact.Text;
        leadDetailDTO.ContactInfo.Email = txtLeadEmail.Text;
        leadDetailDTO.Budget = CommonUtil.getDecimalWithoutExt(txtLeadBudget.Text);
        leadDetailDTO.Source = CommonUIConverter.getMasterControlDTO(drpLeadSource.Text, drpLeadSource.SelectedItem.Text);
    }
    private LeadDetailDTO populateLeadDetailAdd()
    {
    	UserDefinitionDTO userDefDto = getUserDefinitionDTO();
        LeadDetailDTO leadDetailDTO = new LeadDetailDTO();
        leadDetailDTO.ContactInfo = new ContactInfoDTO();
        leadDetailDTO.Property = CommonUIConverter.getPropertyDTO(CommonUtil.getCurrentPropertyDTO(userDefDto).Id.ToString(), null);
        leadDetailDTO.Status = LeadStatus.Open;
        leadDetailDTO.Assignee = CommonUIConverter.getFirmMemberDTO(userDefDto.FirmMember.Id.ToString(), null);
        leadDetailDTO.AssignedDate = DateUtil.getUserLocalDate();
        
        leadDetailDTO.FirmNumber = userDefDto.FirmNumber;
        leadDetailDTO.InsertUser = userDefDto.Username;
        leadDetailDTO.UpdateUser = userDefDto.Username;
        return leadDetailDTO;
    }
    protected void onClickAddLeadBtn(object sender, EventArgs e)
    {
        try
        {
        	initLeadModalFields();
        	initLeadSectionFields(null);
            activeModalHdn.Value = addLeadModal;
        }
        catch (Exception exp)
        {
            log.Error(exp.Message, exp);
            setErrorMessage(CommonUtil.getErrorMessage(exp), commonError);
        }
    }
    protected void saveLead(object sender, EventArgs e)
    {
        try
        {
            if (validateLeadAdd() && validateDuplicateLeadOrEnquiry())
            {
                LeadDetailDTO leadDetailDTO = populateLeadDetailAdd();
                populateAddressFromUI(leadDetailDTO);
                string leadRefNo = enquiryBO.addLeadDetails(leadDetailDTO, getUserDefinitionDTO(), "");
                (this.Master as CSMaster).setNotyMsg(CommonUtil.getNotySuccessMsg(string.Format("New Lead #{0} is added successfully.", leadRefNo)));
                setSearchFilter(null);
                loadLeadSearchGrid();
            }
            else
            {
                activeModalHdn.Value = addLeadModal;
            }
        }
        catch (Exception exp)
        {
            log.Error(exp.Message, exp);
            setErrorMessage(CommonUtil.getErrorMessage(exp), commonError);
        }
    }
    protected void cancelLeadModal(object sender, EventArgs e)
    {
        try
        {
        }
        catch (Exception exp)
        {
            log.Error(exp.Message, exp);
            setErrorMessage(CommonUtil.getErrorMessage(exp), commonError);
        }
    }
    private bool validateLeadAdd()
    {
        Page.Validate(addLeadModalError);
        return Page.IsValid;
    }
    private bool validateDuplicateLeadOrEnquiry() {
    	bool isValid = true;
    	string msg = CommonUIConverter.getDuplicateLeadEnquiryMsg(
    			enquiryBO.fetchDuplicateEnquiryOrLead(
                CommonUtil.getCurrentPropertyDTO(getUserDefinitionDTO()).Id, txtLeadContact.Text, txtLeadAltContact.Text), 
                txtLeadContact.Text, txtLeadAltContact.Text, "");
    	if(!string.IsNullOrWhiteSpace(msg)) {
    		isValid = false;
    		setErrorMessage(msg, addLeadModalError);
    	}
    	return isValid;
    }
    //Lead Modal - End
   
    //Bulk Lead Upload - Start
    protected void onClickUploadLeads(object sender, EventArgs e)
    {
        try
        {
            LeadUploadNavDTO navDTO = new LeadUploadNavDTO();
            navDTO.PrevNavDto = getCurrentPageNavigation();
            Session[Constants.Session.NAV_DTO] = navDTO;
            Response.Redirect(Constants.URL.LEAD_UPLOAD, true);
        }
        catch (Exception exp)
        {
            log.Error(exp.Message, exp);
            setErrorMessage(CommonUtil.getErrorMessage(exp), commonError);
        }
    }
    protected void onClickDownloadTemplate(object sender, EventArgs e)
    {
        try
        {
            List<MasterControlDataDTO> resultsSalutation;
            List<MasterControlDataDTO> resultsSource;
            fetchMaster(out resultsSalutation, out resultsSource);
            if (resultsSalutation == null || resultsSalutation.Count <= 0)
            {
                setErrorMessage("Please add salutations before downloading lead template.", commonError);
            }
            else if (resultsSource == null || resultsSource.Count <= 0)
            {
                setErrorMessage("Please add enquiry source before downloading lead template.", commonError); ;
            }
            else
            {
                setResponse();
                using (ExcelPackage package = new ExcelPackage())
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(CommonUtil.getCurrentPropertyDTO(getUserDefinitionDTO()).Name);

                    using (var range = worksheet.Cells["A1: k1048576"])
                    {
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Top.Color.SetColor(Color.Black);
                        range.Style.Border.Bottom.Color.SetColor(Color.Green);
                        range.Style.Border.Left.Color.SetColor(Color.Blue);
                        range.Style.Border.Right.Color.SetColor(Color.Yellow);
                    }
                    prepareHeader(worksheet);
                    addValidationLists(resultsSalutation, resultsSource, worksheet);
                    CommonUtil.addDateValidation(worksheet, "F2:F1048576");
                    package.Save();
                    using (MemoryStream MyMemoryStream = new MemoryStream())
                    {
                        package.SaveAs(MyMemoryStream);
                        MyMemoryStream.WriteTo(Response.OutputStream);
                        Response.Flush();
                        Response.End();
                    }
                }
            }
        }
        catch (Exception exp)
        {
            log.Error(exp.Message, exp);
            setErrorMessage(CommonUtil.getErrorMessage(exp), commonError);
        }
    }
    private void addValidationLists(List<MasterControlDataDTO> resultsSalutation, List<MasterControlDataDTO> resultsSource, ExcelWorksheet worksheet)
    {
        List<string> propertyList = new List<String>();
        propertyList.Add(CommonUtil.getCurrentPropertyDTO(getUserDefinitionDTO()).Name);
        CommonUtil.addListValidation(worksheet, "A2:A1048576", propertyList);//Property
        CommonUtil.addListValidation(worksheet, "B2:B1048576", CommonUtil.getDropdownItemNames(resultsSalutation));//Salutation
        CommonUtil.addListValidation(worksheet, "K2:K1048576", CommonUtil.getMasterDataNames(resultsSource));//Enquiry Source
    }

    private void setResponse()
    {
        Response.Clear();
        Response.Buffer = true;
        Response.Charset = "";
        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        Response.AddHeader("content-disposition", "attachment;filename=" + CommonUtil.getCurrentPropertyDTO(getUserDefinitionDTO()).Name+" Leads" + ".xlsx");
    }

    private void fetchMaster(out List<MasterControlDataDTO> resultsSalutation, out List<MasterControlDataDTO> resultsSource )
    {
        resultsSalutation = masterDataBO.fetchMasterData(getUserDefinitionDTO().FirmNumber, MasterDataType.SALUTATION.ToString());
        resultsSource = masterDataBO.fetchMasterData(getUserDefinitionDTO().FirmNumber, MasterDataType.ENQUIRY_SOURCE.ToString());
    }

    private void prepareHeader(ExcelWorksheet worksheet)
    {
        worksheet.Cells[1, 1].Value = "Property Name";
        worksheet.Cells[1, 2].Value = "Salutation";
        worksheet.Cells[1, 3].Value = "First Name";
        worksheet.Cells[1, 4].Value = "Middle Name";
        worksheet.Cells[1, 5].Value = "Last Name";
        worksheet.Cells[1, 6].Value = "Lead Date";
        worksheet.Cells[1, 7].Value = "Contact";
        worksheet.Cells[1, 8].Value = "Alternate Contact";
        worksheet.Cells[1, 9].Value = "Email";
        worksheet.Cells[1, 10].Value = "Budget";
        worksheet.Cells[1, 11].Value = "Lead Source";
        using (var range = worksheet.Cells[1, 1, 1, 11])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.Blue);
            range.Style.Font.Color.SetColor(Color.White);
        }
        worksheet.Cells["A1:L1048576"].AutoFilter = true;
        worksheet.Cells.AutoFitColumns(0);
    }
    
    //Bulk Lead Upload - End
}