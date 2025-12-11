CREATE OR REPLACE Package Body pkgf_staff is

  /**************************************************************************************
   * URL redirect to contract detail/batch page together with the input parameters
   * @param p_cntr_ctr: Contract (Centre Part)
   * @param p_cntr_yr: Contract (Year Part)
   * @param p_cntr_sqn: Contract (Sequence Part)
   * @param p_msg: Message
   * @param p_mode: Mode of action
   **************************************************************************************/
  procedure staff_redirect (
    p_stf_no hr_staff.stf_no%type,
    p_msg_type varchar2,
    p_msg varchar2
  ) as
  begin
    set_hdr_msg(p_msg=>p_msg,p_msg_type=>p_msg_type);
    htp.prn('
      <script language="javascript">
      <!--
         location.href="'||get_access_str||'.pkgf_staff.find_rec?p_stf_no='||p_stf_no||'";
      //-->
      </script>
    ');
  end staff_redirect;

  /*********************************************************************************
   * This function constructs a record of staff in a record type "pkg_rec.rec_Staff"
   * Input parameters are the data field in table tbl_staff
   * @return: a record of staff
   *********************************************************************************/
  function const_rec (
    p_stf_no            in hr_staff.stf_no%type:=null,
    p_stf_name          in hr_staff.stf_name%type,
    p_stf_cname         in varchar2,
    p_stf_hkid          in hr_staff.stf_hkid%type,
    p_stf_pp_no         in hr_staff.stf_pp_no%type,
    p_stf_pp_iscnty     in hr_staff.stf_pp_iscnty%type,
    p_stf_nat           in hr_staff.stf_nat%type,
    p_stf_dob           in hr_staff.stf_dob%type,
    p_stf_sex           in hr_staff.stf_sex%type,
    p_stf_marital_stat  in hr_staff.stf_marital_stat%type,
    p_stf_addr1         in hr_staff.stf_addr1%type,
    p_stf_addr2         in hr_staff.stf_addr2%type,
    p_stf_addr3         in hr_staff.stf_addr3%type,
    p_stf_addr4         in hr_staff.stf_addr4%type,
    p_stf_addr_area     in hr_staff.stf_addr_area%type,
    p_stf_ac_bnk_code   in hr_staff.stf_ac_bnk_code%type,
    p_stf_ac_code       in hr_staff.stf_ac_code%type,
    p_stf_sps_name      in hr_staff.stf_sps_name%type,
    p_stf_sps_hkid      in hr_staff.stf_sps_hkid%type,
    p_stf_sps_pp_no     in hr_staff.stf_sps_pp_no%type,
    p_stf_sps_pp_iscnty in hr_staff.stf_sps_pp_iscnty%type,
    p_stf_sps_health    in hr_staff.stf_sps_health%type,
    p_stf_dad_name      in hr_staff.stf_dad_name%type,
    p_stf_dad_hkid      in hr_staff.stf_dad_hkid%type,
    p_stf_dad_pp_no     in hr_staff.stf_dad_pp_no%type,
    p_stf_dad_pp_iscnty in hr_staff.stf_dad_pp_iscnty%type,
    p_stf_dad_health    in hr_staff.stf_dad_health%type,
    p_stf_mom_name      in hr_staff.stf_mom_name%type,
    p_stf_mom_hkid      in hr_staff.stf_mom_hkid%type,
    p_stf_mom_pp_no     in hr_staff.stf_mom_pp_no%type,
    p_stf_mom_pp_iscnty in hr_staff.stf_mom_pp_iscnty%type,
    p_stf_mom_health    in hr_staff.stf_mom_health%type,
    p_stf_empvisa_xdate in hr_staff.stf_empvisa_xdate%type,
    p_stf_permitno      in hr_staff.stf_permitno%type,
    p_stf_permit_xdate  in hr_staff.stf_permit_xdate%type,
    p_stf_actn          in hr_staff.stf_actn%type,
    p_stf_actndate      in hr_staff.stf_actndate%type,
    p_stf_actnuser      in hr_staff.stf_actnuser%type,
    p_timestamp         in hr_staff.timestamp%type:=null
    --------------------------------------------- -----
    -- AngelChan@20250212 added with the EMPF name handle 
    ,p_stf_surname       in hr_staff.stf_surname%type,
    p_stf_givenname     in hr_staff.stf_givenname%type,
    p_stf_phone1areacode in hr_staff.stf_phone1areacode%type,
    p_stf_email         in hr_staff.stf_email%type,
    p_stf_phone1        in hr_staff.stf_phone1%type
    ------------------------------------------------------
  ) return pkg_rec.rec_staff as
    v_rec pkg_rec.rec_staff;
  begin
    v_rec.stf_no:=p_stf_no;
    v_rec.stf_name:=p_stf_name;
    v_rec.stf_cname:=p_stf_cname;
    v_rec.stf_hkid:=p_stf_hkid;
    v_rec.stf_pp_no:=p_stf_pp_no;
    v_rec.stf_pp_iscnty:=p_stf_pp_iscnty;
    v_rec.stf_nat:=p_stf_nat;
    v_rec.stf_dob:=p_stf_dob;
    v_rec.stf_sex:=p_stf_sex;
    v_rec.stf_marital_stat:=p_stf_marital_stat;
    v_rec.stf_addr1:=p_stf_addr1;
    v_rec.stf_addr2:=p_stf_addr2;
    v_rec.stf_addr3:=p_stf_addr3;
    v_rec.stf_addr4:=p_stf_addr4;
    v_rec.stf_addr_area:=p_stf_addr_area;
    v_rec.stf_ac_bnk_code:=p_stf_ac_bnk_code;
    v_rec.stf_ac_code:=p_stf_ac_code;
    v_rec.stf_sps_name:=p_stf_sps_name;
    v_rec.stf_sps_hkid:=p_stf_sps_hkid;
    v_rec.stf_sps_pp_no:=p_stf_sps_pp_no;
    v_rec.stf_sps_pp_iscnty:=p_stf_sps_pp_iscnty;
    v_rec.stf_sps_health:=p_stf_sps_health;
    v_rec.stf_dad_name:=p_stf_dad_name;
    v_rec.stf_dad_hkid:=p_stf_dad_hkid;
    v_rec.stf_dad_pp_no:=p_stf_dad_pp_no;
    v_rec.stf_dad_pp_iscnty:=p_stf_dad_pp_iscnty;
    v_rec.stf_dad_health:=p_stf_dad_health;
    v_rec.stf_mom_name:=p_stf_mom_name;
    v_rec.stf_mom_hkid:=p_stf_mom_hkid;
    v_rec.stf_mom_pp_no:=p_stf_mom_pp_no;
    v_rec.stf_mom_pp_iscnty:=p_stf_mom_pp_iscnty;
    v_rec.stf_mom_health:=p_stf_mom_health;
    v_rec.stf_empvisa_xdate:=p_stf_empvisa_xdate;
    v_rec.stf_permitno:=p_stf_permitno;
    v_rec.stf_permit_xdate:=p_stf_permit_xdate;
    v_rec.stf_actn:=p_stf_actn;
    v_rec.stf_actndate:=p_stf_actndate;
    v_rec.stf_actnuser:=p_stf_actnuser;
    v_rec.timestamp:=p_timestamp;
    -----------------------------------------------------
    -- AngelChan@20250212 added with the EMPF name handle 
    v_rec.stf_surname := p_stf_surname;
    v_rec.stf_givenname := p_stf_givenname;
    v_rec.stf_phone1areacode := p_stf_phone1areacode;
    v_rec.stf_email := p_stf_email;
    v_rec.stf_phone1 := p_stf_phone1;
    -----------------------------------------------------
    return v_rec;
  end const_rec;

  /*******************************************************************************************
   * This procedure acts as a communication bridge between this package and the web forms
   * Input parameters are data fields in table tbl_staff exception p_ts
   * @param: p_ts: timestamp in character format defined in procedure pkg_format.char2datetime
   * @param: p_mode: determine the action to be performed
   *******************************************************************************************/
  procedure form (
    p_stf_no            in hr_staff.stf_no%type :=null,
    p_stf_name          in hr_staff.stf_name%type,
    p_stf_cname         in varchar2,
    p_stf_hkid          in hr_staff.stf_hkid%type,
    p_stf_pp_no         in hr_staff.stf_pp_no%type,
    p_stf_pp_iscnty     in hr_staff.stf_pp_iscnty%type,
    p_stf_nat           in hr_staff.stf_nat%type,
    p_stf_dob           in hr_staff.stf_dob%type,
    p_stf_sex           in hr_staff.stf_sex%type,
    p_stf_marital_stat  in hr_staff.stf_marital_stat%type,
    p_stf_addr1         in hr_staff.stf_addr1%type,
    p_stf_addr2         in hr_staff.stf_addr2%type,
    p_stf_addr3         in hr_staff.stf_addr3%type,
    p_stf_addr4         in hr_staff.stf_addr4%type,
    p_stf_addr_area     in hr_staff.stf_addr_area%type,
    p_stf_ac_bnk_code   in hr_staff.stf_ac_bnk_code%type,
    p_stf_ac_code       in hr_staff.stf_ac_code%type,
    p_stf_sps_name      in hr_staff.stf_sps_name%type,
    p_stf_sps_hkid      in hr_staff.stf_sps_hkid%type,
    p_stf_sps_pp_no     in hr_staff.stf_sps_pp_no%type,
    p_stf_sps_pp_iscnty in hr_staff.stf_sps_pp_iscnty%type,
    p_stf_sps_health    in hr_staff.stf_sps_health%type,
    p_stf_dad_name      in hr_staff.stf_dad_name%type,
    p_stf_dad_hkid      in hr_staff.stf_dad_hkid%type,
    p_stf_dad_pp_no     in hr_staff.stf_dad_pp_no%type,
    p_stf_dad_pp_iscnty in hr_staff.stf_dad_pp_iscnty%type,
    p_stf_dad_health    in hr_staff.stf_dad_health%type,
    p_stf_mom_name      in hr_staff.stf_mom_name%type,
    p_stf_mom_hkid      in hr_staff.stf_mom_hkid%type,
    p_stf_mom_pp_no     in hr_staff.stf_mom_pp_no%type,
    p_stf_mom_pp_iscnty in hr_staff.stf_mom_pp_iscnty%type,
    p_stf_mom_health    in hr_staff.stf_mom_health%type,
    p_stf_empvisa_xdate in hr_staff.stf_empvisa_xdate%type,
    p_stf_permitno      in hr_staff.stf_permitno%type,
    p_stf_permit_xdate  in hr_staff.stf_permit_xdate%type,
    p_stf_actn          in hr_staff.stf_actn%type,
    p_stf_actndate      in hr_staff.stf_actndate%type,
    p_stf_actnuser      in hr_staff.stf_actnuser%type,
    p_ts		in varchar2:=null,
    p_form_ts 		in varchar2:=null,
    p_mode              in varchar2
    --------------------------------------------- -----
    -- AngelChan@20250212 added with the EMPF name handle 
    ,p_stf_surname       in hr_staff.stf_surname%type,
    p_stf_givenname      in hr_staff.stf_givenname%type,
    p_stf_phone1areacode in hr_staff.stf_phone1areacode%type default null,
    p_stf_email          in hr_staff.stf_email%type default null,
    p_stf_phone1         in hr_staff.stf_phone1%type  default null
    ------------------------------------------------------
  ) as
    v_rec pkg_rec.rec_staff;
    v_mode varchar2(20);
    e_invalid_rec exception;
  begin
    v_mode:=trim(lower(p_mode));

    v_rec:=const_rec(  -- Construct a staff record from input parameters
      p_stf_no=>p_stf_no,
      p_stf_name=>p_stf_name,
      p_stf_cname=>p_stf_cname,
      p_stf_hkid=>p_stf_hkid,
      p_stf_pp_no=>p_stf_pp_no,
      p_stf_pp_iscnty=>p_stf_pp_iscnty,
      p_stf_nat=>p_stf_nat,
      p_stf_dob=>p_stf_dob,
      p_stf_sex=>p_stf_sex,
      p_stf_marital_stat=>p_stf_marital_stat,
      p_stf_addr1=>p_stf_addr1,
      p_stf_addr2=>p_stf_addr2,
      p_stf_addr3=>p_stf_addr3,
      p_stf_addr4=>p_stf_addr4,
      p_stf_addr_area=>p_stf_addr_area,
      p_stf_ac_bnk_code=>p_stf_ac_bnk_code,
      p_stf_ac_code=>p_stf_ac_code,
      p_stf_sps_name=>p_stf_sps_name,
      p_stf_sps_hkid=>p_stf_sps_hkid,
      p_stf_sps_pp_no=>p_stf_sps_pp_no,
      p_stf_sps_pp_iscnty=>p_stf_sps_pp_iscnty,
      p_stf_sps_health=>p_stf_sps_health,
      p_stf_dad_name=>p_stf_dad_name,
      p_stf_dad_hkid=>p_stf_dad_hkid,
      p_stf_dad_pp_no=>p_stf_dad_pp_no,
      p_stf_dad_pp_iscnty=>p_stf_dad_pp_iscnty,
      p_stf_dad_health=>p_stf_dad_health,
      p_stf_mom_name=>p_stf_mom_name,
      p_stf_mom_hkid=>p_stf_mom_hkid,
      p_stf_mom_pp_no=>p_stf_mom_pp_no,
      p_stf_mom_pp_iscnty=>p_stf_mom_pp_iscnty,
      p_stf_mom_health=>p_stf_mom_health,
      p_stf_empvisa_xdate=>p_stf_empvisa_xdate,
      p_stf_permitno=>p_stf_permitno,
      p_stf_permit_xdate=>p_stf_permit_xdate,
      p_stf_actn=>p_stf_actn,
      p_stf_actndate=>p_stf_actndate,
      p_stf_actnuser=>p_stf_actnuser,
      p_timestamp=>pkg_format.char2datetime(p_ts)
      --------------------------------------------- -----
      -- AngelChan@20250212 added with the EMPF name handle 
      ,p_stf_surname=>p_stf_surname,
      p_stf_givenname=>p_stf_givenname,
      p_stf_phone1areacode=>p_stf_phone1areacode,
      p_stf_email=>p_stf_email,
      p_stf_phone1=>p_stf_phone1
      ------------------------------------------------------
    );

    ------ Redirect to the appropriate procedure according to the action (p_mode) specified -----
    if v_mode='insert' then
      create_tx(v_rec);
    elsif v_mode='insert_real' then
      create_tx_real(v_rec);
    elsif v_mode='update' then
      update_tx(v_rec);
    elsif v_mode like 'update_real%' then
      update_tx_real(v_rec,v_mode);
    end if;
    ---------------------------------------------------------------------------------------------
  exception
    when e_invalid_rec then
      set_hdr_msg(p_msg_type=>'E');
      staff_dtls(p_msg=>'Invalid Record Found');
    when others then
      set_hdr_msg(p_msg_type=>'E');
      staff_dtls(p_msg=>sqlerrm);
  end form;

  /****************************************************************************************
   * Construct an HTML form for automcatic submission to pkgf_staff.form
   ****************************************************************************************/
  procedure const_virtual_form (p_rec pkg_rec.rec_staff, p_mode varchar2) as
  begin
    htp.p('
      <form name="f1" action="'||get_access_str||'.pkgf_staff.form" method="post">
        <input type="hidden" name="p_stf_no" value="'||p_rec.stf_no||'"/>
        <input type="hidden" name="p_stf_name" value="'||p_rec.stf_name||'"/>
        <input type="hidden" name="p_stf_cname" value="'||p_rec.stf_cname||'"/>
        <input type="hidden" name="p_stf_hkid" value="'||p_rec.stf_hkid||'"/>
        <input type="hidden" name="p_stf_pp_no" value="'||p_rec.stf_pp_no||'"/>
        <input type="hidden" name="p_stf_pp_iscnty" value="'||p_rec.stf_pp_iscnty||'"/>
        <input type="hidden" name="p_stf_nat" value="'||p_rec.stf_nat||'"/>
        <input type="hidden" name="p_stf_dob" value="'||p_rec.stf_dob||'"/>
        <input type="hidden" name="p_stf_sex" value="'||p_rec.stf_sex||'"/>
        <input type="hidden" name="p_stf_marital_stat" value="'||p_rec.stf_marital_stat||'"/>
        <input type="hidden" name="p_stf_addr1" value="'||p_rec.stf_addr1||'"/>
        <input type="hidden" name="p_stf_addr2" value="'||p_rec.stf_addr2||'"/>
        <input type="hidden" name="p_stf_addr3" value="'||p_rec.stf_addr3||'"/>
        <input type="hidden" name="p_stf_addr4" value="'||p_rec.stf_addr4||'"/>
        <input type="hidden" name="p_stf_addr_area" value="'||p_rec.stf_addr_area||'"/>
        <input type="hidden" name="p_stf_ac_bnk_code" value="'||p_rec.stf_ac_bnk_code||'"/>
        <input type="hidden" name="p_stf_ac_code" value="'||p_rec.stf_ac_code||'"/>
        <input type="hidden" name="p_stf_sps_name" value="'||p_rec.stf_sps_name||'"/>
        <input type="hidden" name="p_stf_sps_hkid" value="'||p_rec.stf_sps_hkid||'"/>
        <input type="hidden" name="p_stf_sps_pp_no" value="'||p_rec.stf_sps_pp_no||'"/>
        <input type="hidden" name="p_stf_sps_pp_iscnty" value="'||p_rec.stf_sps_pp_iscnty||'"/>
        <input type="hidden" name="p_stf_sps_health" value="'||p_rec.stf_sps_health||'"/>
        <input type="hidden" name="p_stf_dad_name" value="'||p_rec.stf_dad_name||'"/>
        <input type="hidden" name="p_stf_dad_hkid" value="'||p_rec.stf_dad_hkid||'"/>
        <input type="hidden" name="p_stf_dad_pp_no" value="'||p_rec.stf_dad_pp_no||'"/>
        <input type="hidden" name="p_stf_dad_pp_iscnty" value="'||p_rec.stf_dad_pp_iscnty||'"/>
        <input type="hidden" name="p_stf_dad_health" value="'||p_rec.stf_dad_health||'"/>
        <input type="hidden" name="p_stf_mom_name" value="'||p_rec.stf_mom_name||'"/>
        <input type="hidden" name="p_stf_mom_hkid" value="'||p_rec.stf_mom_hkid||'"/>
        <input type="hidden" name="p_stf_mom_pp_no" value="'||p_rec.stf_mom_pp_no||'"/>
        <input type="hidden" name="p_stf_mom_pp_iscnty" value="'||p_rec.stf_mom_pp_iscnty||'"/>
        <input type="hidden" name="p_stf_mom_health" value="'||p_rec.stf_mom_health||'"/>
        <input type="hidden" name="p_stf_empvisa_xdate" value="'||p_rec.stf_empvisa_xdate||'"/>
        <input type="hidden" name="p_stf_permitno" value="'||p_rec.stf_permitno||'"/>
        <input type="hidden" name="p_stf_permit_xdate" value="'||p_rec.stf_permit_xdate||'"/>
        <input type="hidden" name="p_stf_actn" value="'||p_rec.stf_actn||'"/>
        <input type="hidden" name="p_stf_actndate" value="'||p_rec.stf_actndate||'"/>
        <input type="hidden" name="p_stf_actnuser" value="'||p_rec.stf_actnuser||'"/>
        <input type="hidden" name="p_ts" value="'||pkg_format.datetime2char(p_rec.timestamp)||'"/>
        <input type="hidden" name="p_mode" value="'||p_mode||'"/>
        <!-- Angel Chan added 20250217 ---------------------------------------------->
        <input type="hidden" name="p_stf_surname" value="'||p_rec.stf_surname||'"/>
        <input type="hidden" name="p_stf_givenname" value="'||p_rec.stf_givenname||'"/>
        <input type="hidden" name="p_stf_email" value="'||p_rec.stf_email||'"/>
        <input type="hidden" name="p_stf_phone1" value="'||p_rec.stf_phone1||'"/>
        <input type="hidden" name="p_stf_phone1areacode" value="'||p_rec.stf_phone1areacode||'"/>
        <!-- END Angel Chan added 20250217 ---------------------------------------------->
      </form>
      <script language="javascript">
      <!--
         function replaceChar(value,o_char,n_char) {
           s=new String(value);
           ss="";
           for (i=0;i<s.length;i++) {
             if (s.charAt(i)==o_char) ss+=n_char;
             else ss+=s.charAt(i);
           }
           return ss;
         }
         f1.p_stf_cname.value=unescape(replaceChar(f1.p_stf_cname.value,"\\","%"));
         f1.submit();
      //-->
      </script>
    ');
  end const_virtual_form;


  /*******************************************************************************************
   * Creates a simple HTML form to decode the Chinese field
   * then automatically submit the form to an "insert procedure"
   * @param: p_rec: Staff record
   *******************************************************************************************/
  procedure create_tx (p_rec pkg_rec.rec_staff) is
  begin
    const_virtual_form(p_rec,'insert_real');
  end create_tx;

  /*******************************************************************************************
   * Perform actual insertion action of a staff record
   * @param: p_rec: Staff record
   *******************************************************************************************/
  procedure create_tx_real(p_rec pkg_rec.rec_staff) is
    v_stfno hr_staff.stf_no%type;
    v_owner varchar2(100);
    v_log_msg hr_app_log.apl_details%type;
    v_msg varchar2(1000);
    ---------------------------------------------------------------------------------------
    -- Conrad Kwong @ 2025-06-10, Form# HRIS-250xx, modify for eMPF
    v_surname hr_staff.stf_surname%type := trim(replace(upper(p_rec.stf_surname),'  ',' '));
    v_givenname hr_staff.stf_givenname%type := trim(replace(upper(p_rec.stf_givenname),'  ',' '));
    v_stf_name hr_staff.stf_name%type := trim(replace(v_surname||' '||v_givenname,'  ',' '));
    ---------------------------------------------------------------------------------------
  begin
    v_owner:=get_access_str;
    if pkg_validate.hasErrorMsg(pkg_validate.validation('insert',p_rec)) then
      v_msg:='Inappropriate Insertion Action';
      error_page(p_stf_no=>p_rec.stf_no,p_char_msg_type=>'E',p_char_msg=>v_msg);
      return;
    end if;
    -- Get a new staff number
    pkg_systbl.get_new_stfno(v_stfno);
    insert into hr_staff (
      stf_no,
      stf_name,
      stf_cname,
      stf_hkid,
      stf_pp_no,
      stf_pp_iscnty,
      stf_nat,
      stf_dob,
      stf_sex,
      stf_marital_stat,
      stf_addr1,
      stf_addr2,
      stf_addr3,
      stf_addr4,
      stf_addr_area,
      stf_ac_bnk_code,
      stf_ac_code,
      stf_sps_name,
      stf_sps_hkid,
      stf_sps_pp_no,
      stf_sps_pp_iscnty,
      stf_sps_health,
      stf_dad_name,
      stf_dad_hkid,
      stf_dad_pp_no,
      stf_dad_pp_iscnty,
      stf_dad_health,
      stf_mom_name,
      stf_mom_hkid,
      stf_mom_pp_no,
      stf_mom_pp_iscnty,
      stf_mom_health,
      stf_empvisa_xdate,
      stf_permitno,
      stf_permit_xdate,
      stf_actn,
      stf_actndate,
      stf_actnuser,
      timestamp,
      -------------------
      -- Angel Chan @ 20250217 added for MPF 
      stf_surname,
      stf_givenname,
      stf_phone1,
      stf_email,
      stf_phone1AreaCode
      -------------------
    ) values (
      v_stfno,
      ---------------------------------------------------------------------------------------
      -- Conrad Kwong @ 2025-06-10, Form# HRIS-250xx, modify for eMPF
      --p_rec.stf_name,
      v_stf_name,
      ---------------------------------------------------------------------------------------
      p_rec.stf_cname,
      p_rec.stf_hkid,
      p_rec.stf_pp_no,
      p_rec.stf_pp_iscnty,
      p_rec.stf_nat,
      p_rec.stf_dob,
      p_rec.stf_sex,
      p_rec.stf_marital_stat,
      p_rec.stf_addr1,
      p_rec.stf_addr2,
      p_rec.stf_addr3,
      p_rec.stf_addr4,
      p_rec.stf_addr_area,
      p_rec.stf_ac_bnk_code,
      p_rec.stf_ac_code,
      p_rec.stf_sps_name,
      p_rec.stf_sps_hkid,
      p_rec.stf_sps_pp_no,
      p_rec.stf_sps_pp_iscnty,
      p_rec.stf_sps_health,
      p_rec.stf_dad_name,
      p_rec.stf_dad_hkid,
      p_rec.stf_dad_pp_no,
      p_rec.stf_dad_pp_iscnty,
      p_rec.stf_dad_health,
      p_rec.stf_mom_name,
      p_rec.stf_mom_hkid,
      p_rec.stf_mom_pp_no,
      p_rec.stf_mom_pp_iscnty,
      p_rec.stf_mom_health,
      p_rec.stf_empvisa_xdate,
      p_rec.stf_permitno,
      p_rec.stf_permit_xdate,
      'insert',
      sysdate,
      user,
      sysdate
      -------------------------------------------
      -- Angel Chan @ 20250217 added for MPF 
      ---------------------------------------------------------------------------------------
      -- Conrad Kwong @ 2025-06-10, Form# HRIS-250xx, modify for eMPF
      --,p_rec.stf_surname,
      --p_rec.stf_givenname,
      ,v_surname,
      v_givenname,
      ---------------------------------------------------------------------------------------
      p_rec.stf_phone1,
      p_rec.stf_email,
      p_rec.stf_phone1areacode
      -------------------------------------------
    ) returning stf_no into v_stfno;

    v_log_msg:='A new staff, staff no. <a href="'||v_owner||'.f_submain?p_stf_no='||v_stfno||'">'||v_stfno||'</a> is created.';
    insert into hr_app_log(
      apl_prog_id,
      apl_prog_name,
      apl_details,
      apl_msg_typ
    ) values(
      'pkgf_staff.create_tx',
      'New Staff Creation',
      v_log_msg,
      'I'
    );
    -- set staff number cookie value as new staff number and contract number cookie expire
    pkg_cookie.set_stfno(p_stfno => v_stfno);
    f_submain(p_stf_no => v_stfno);
  exception
    when others then
      v_log_msg:='Error during creation of a new staff due to : ' || sqlerrm;
      insert into hr_app_log(
        apl_prog_id,
        apl_prog_name,
        apl_details,
        apl_msg_typ
      ) values(
        'pkgf_staff.create_tx',
        'New Staff Creation',
        v_log_msg,
        'E'
      );
      error_page(p_stf_no=>p_rec.stf_no,p_char_msg_type=>'E',p_char_msg=>v_log_msg);
  end create_tx_real;


  /*******************************************************************************************
   * Creates a simple HTML form to decode the Chinese field
   * then automatically submit the form to an "update procedure"
   * @param: p_rec: Staff record
   *******************************************************************************************/
  procedure update_tx (p_rec pkg_rec.rec_staff) is
  begin
    const_virtual_form(p_rec,'update_real');
  end update_tx;

  /*******************************************************************************************
   * Perform a actual update action of a staff record
   * @param: p_rec: Staff record
   *******************************************************************************************/
  procedure update_tx_real (p_rec pkg_rec.rec_staff, p_mode in varchar2:='update_real') is
    v_rec_old hr_staff%rowtype;
    e_data_outdated exception;
    v_msg varchar2(1000);
    v_log_msg varchar2(1000);
    v_owner varchar2(100);
    ---------------------------------------------------------------------------------------
    -- Conrad Kwong @ 2025-06-10, Form# HRIS-250xx, modify for eMPF
    v_surname hr_staff.stf_surname%type := trim(replace(upper(p_rec.stf_surname),'  ',' '));
    v_givenname hr_staff.stf_givenname%type := trim(replace(upper(p_rec.stf_givenname),'  ',' '));
    v_stf_name hr_staff.stf_name%type := trim(replace(v_surname||' '||v_givenname,'  ',' '));
    ---------------------------------------------------------------------------------------
  begin
    v_owner:=get_access_str;
    if not auth_mod_stf_dtls(p_rec.stf_no) then
      v_msg:='Unauthorize to modify this staff record';
      staff_redirect(p_stf_no=>p_rec.stf_no,p_msg_type=>'E',p_msg=>v_msg);
      return;
    end if;

    if pkg_validate.hasErrorMsg(pkg_validate.validation('update',p_rec)) then
      v_msg:='Inappropriate Update Action';
      error_page(p_stf_no=>p_rec.stf_no,p_char_msg_type=>'E',p_char_msg=>v_msg);
      return;
    end if;

    select * into v_rec_old from hr_staff where stf_no=p_rec.stf_no;
    if v_rec_old.timestamp<>p_rec.timestamp then
      v_msg:='Update Failure: The record was modified by other. The latest change is shown as follows. You may try to update again if neccessary';
      raise e_data_outdated;
    end if;

    if allow_chg_bnkac(p_rec.stf_no) then
      update hr_staff
      set
        ---------------------------------------------------------------------------------------
        -- Conrad Kwong @ 2025-06-10, Form# HRIS-250xx, modify for eMPF
        --stf_name=p_rec.stf_name,
        stf_name=v_stf_name,
        ---------------------------------------------------------------------------------------
        stf_cname=p_rec.stf_cname,
        stf_hkid=p_rec.stf_hkid,
        stf_pp_no=p_rec.stf_pp_no,
        stf_pp_iscnty =p_rec.stf_pp_iscnty,
        stf_nat=p_rec.stf_nat,
        stf_dob=p_rec.stf_dob,
        stf_sex=p_rec.stf_sex,
        stf_marital_stat=p_rec.stf_marital_stat,
        stf_addr1=p_rec.stf_addr1,
        stf_addr2=p_rec.stf_addr2,
        stf_addr3=p_rec.stf_addr3,
        stf_addr4=p_rec.stf_addr4,
        stf_addr_area=p_rec.stf_addr_area,
        stf_ac_bnk_code=p_rec.stf_ac_bnk_code,
        stf_ac_code=p_rec.stf_ac_code,
        stf_sps_name=p_rec.stf_sps_name,
        stf_sps_hkid=p_rec.stf_sps_hkid,
        stf_sps_pp_no=p_rec.stf_sps_pp_no,
        stf_sps_pp_iscnty=p_rec.stf_sps_pp_iscnty,
        stf_sps_health=p_rec.stf_sps_health,
        stf_dad_name=p_rec.stf_dad_name,
        stf_dad_hkid=p_rec.stf_dad_hkid,
        stf_dad_pp_no=p_rec.stf_dad_pp_no,
        stf_dad_pp_iscnty=p_rec.stf_dad_pp_iscnty,
        stf_dad_health=p_rec.stf_dad_health,
        stf_mom_name=p_rec.stf_mom_name,
        stf_mom_hkid=p_rec.stf_mom_hkid,
        stf_mom_pp_no=p_rec.stf_mom_pp_no,
        stf_mom_pp_iscnty=p_rec.stf_mom_pp_iscnty,
        stf_mom_health=p_rec.stf_mom_health,
        stf_empvisa_xdate=p_rec.stf_empvisa_xdate,
        stf_permitno =p_rec.stf_permitno ,
        stf_permit_xdate=p_rec.stf_permit_xdate,
        stf_actn='update',
        stf_actndate=sysdate,
        stf_actnuser=user,
        timestamp=sysdate
        -------------------------------------------
        -- Angel Chan @ 20250217 added for MPF 
        ---------------------------------------------------------------------------------------
        -- Conrad Kwong @ 2025-06-10, Form# HRIS-250xx, modify for eMPF
        --,stf_surname = p_rec.stf_surname,
        --stf_givenname = p_rec.stf_givenname,
        ,stf_surname = v_surname,
        stf_givenname = v_givenname,
        ---------------------------------------------------------------------------------------
        stf_phone1 = p_rec.stf_phone1,
        stf_email = p_rec.stf_email,
        stf_phone1areacode = p_rec.stf_phone1areacode
        -------------------------------------------
      where stf_no=p_rec.stf_no;
    else
      update hr_staff
      set
        stf_cname=p_rec.stf_cname,
        stf_hkid=p_rec.stf_hkid,
        stf_pp_no=p_rec.stf_pp_no,
        stf_pp_iscnty =p_rec.stf_pp_iscnty,
        stf_nat=p_rec.stf_nat,
        stf_dob=p_rec.stf_dob,
        stf_sex=p_rec.stf_sex,
        stf_marital_stat=p_rec.stf_marital_stat,
        stf_addr1=p_rec.stf_addr1,
        stf_addr2=p_rec.stf_addr2,
        stf_addr3=p_rec.stf_addr3,
        stf_addr4=p_rec.stf_addr4,
        stf_addr_area=p_rec.stf_addr_area,
        stf_sps_name=p_rec.stf_sps_name,
        stf_sps_hkid=p_rec.stf_sps_hkid,
        stf_sps_pp_no=p_rec.stf_sps_pp_no,
        stf_sps_pp_iscnty=p_rec.stf_sps_pp_iscnty,
        stf_sps_health=p_rec.stf_sps_health,
        stf_dad_name=p_rec.stf_dad_name,
        stf_dad_hkid=p_rec.stf_dad_hkid,
        stf_dad_pp_no=p_rec.stf_dad_pp_no,
        stf_dad_pp_iscnty=p_rec.stf_dad_pp_iscnty,
        stf_dad_health=p_rec.stf_dad_health,
        stf_mom_name=p_rec.stf_mom_name,
        stf_mom_hkid=p_rec.stf_mom_hkid,
        stf_mom_pp_no=p_rec.stf_mom_pp_no,
        stf_mom_pp_iscnty=p_rec.stf_mom_pp_iscnty,
        stf_mom_health=p_rec.stf_mom_health,
        stf_empvisa_xdate=p_rec.stf_empvisa_xdate,
        stf_permitno =p_rec.stf_permitno ,
        stf_permit_xdate=p_rec.stf_permit_xdate,
        stf_actn='update',
        stf_actndate=sysdate,
        stf_actnuser=user,
        timestamp=sysdate
        -------------------------------------------
        -- Angel Chan @ 20250217 added for MPF 
        ---------------------------------------------------------------------------------------
        -- Conrad Kwong @ 2025-06-10, Form# HRIS-250xx, modify for eMPF
        --,stf_surname = p_rec.stf_surname,
        --stf_givenname = p_rec.stf_givenname,
        ---------------------------------------------------------------------------------------
        ,stf_phone1 = p_rec.stf_phone1,
        stf_email = p_rec.stf_email,
        stf_phone1areacode = p_rec.stf_phone1areacode
        -------------------------------------------
      where stf_no=p_rec.stf_no;
    end if;


    if v_rec_old.stf_ac_bnk_code<>p_rec.stf_ac_bnk_code or v_rec_old.stf_ac_code<>p_rec.stf_ac_code then
      v_log_msg:='Bank Account code of staff (Staff No. <a href="'||v_owner||'.f_submain?p_stf_no='||v_rec_old.stf_no||'">' || v_rec_old.stf_no || '</a>) changed';
      add_log('Update staff Program',v_log_msg,'I');
    end if;

    if trim(v_rec_old.stf_name)<>trim(p_rec.stf_name) then
      v_log_msg:='Staff name (Staff No. <a href="'||v_owner||'.f_submain?p_stf_no='||v_rec_old.stf_no||'">' || v_rec_old.stf_no || '</a>) changed';
      add_log('Update staff Program',v_log_msg,'I');
    end if;

    owa_util.redirect_url(get_access_str||'.pkgf_staff.find_rec?p_stf_no='||p_rec.stf_no);

  exception
    when e_data_outdated then
      staff_redirect(p_stf_no=>p_rec.stf_no,p_msg_type=>'E',p_msg=>v_msg);
  end update_tx_real;


  /*******************************************************************************************
   * Find a staff record according to staff ID/staff HKID/ staff Passport No./ staff name
   * If both ID and staff name are specified, the search action is based on the ID only.
   * @param p_idtype: Type of ID -- S: Staff ID, I: HKID, P: Passport No.
   * @param p_idno: Actual ID value
   * @param p_stf_name: Staff Name
   *******************************************************************************************/
  procedure find_rec(
    p_idtype in char :=null,
    p_idno in char :=null,
    p_stf_name in hr_staff.stf_name%type:=null
  ) is
    v_stfno hr_staff.stf_no%type;
    v_msg varchar2(1000);
  begin
    --select stf_no into v_stfno from hr_staff where xxx
    -- case 1: searched by staff id or ppid
    -- show data according to the user's privilege

    if lower(p_idtype)='s' and p_idno is not null then
      v_msg:='No record found by this staff number';
      select stf_no into v_stfno
      from hr_staff
      where trim(stf_no)=trim(p_idno);
    elsif lower(p_idtype)='i' and p_idno is not null then
      v_msg:='No record found by this Hong Kong ID number';
      if isHKIDExists(p_idno)=false then
        raise no_data_found;
      end if;
      select stf_no into v_stfno
      from hr_staff
      where trim(stf_hkid)=trim(p_idno);
    elsif lower(p_idtype)='p' and p_idno is not null then
      v_msg:='No record found by this passport number';
      if isPP_NOExists(p_idno)=false then
        raise no_data_found;
      end if;
      select stf_no into v_stfno
      from hr_staff
      where trim(stf_pp_no)=trim(p_idno);
    -- case 2: searched by staff name
    -- show data according to the user's privilege
    elsif p_stf_name is not null then
      if auth_vw_all_stf then
        v_msg:='No record found by this staff name';
        select stf_no into v_stfno
        from hr_staff
        where (upper(trim(stf_name)) like upper(trim(p_stf_name)))
                 or
              trim(stf_cname) like trim(p_stf_name);

      else
        v_msg:='You can only find staff that has part-time payroll contracts under your centre when you search staff by name';
        select stf_no into v_stfno
        from hr_staff inner join hr_user_ptcntr on stf_no=pct_stfno
        where (upper(trim(stf_name)) like upper(trim(p_stf_name)))
                 or
              trim(stf_cname) like trim(p_stf_name);
      end if;
    end if;
    -- send a cookie containing the staff number to the client
    pkg_cookie.set_stfno(v_stfno);
    f_submain(p_stf_no=>v_stfno);
  exception
    when no_data_found then
      pkg_cookie.set_stfno(null);
      staff_list (p_msg=>v_msg);
    when too_many_rows then
      pkg_cookie.set_stfno(null);
      staff_list (p_stf_name=>p_stf_name);
  end find_rec;

  /*******************************************************************************************
   * Find a staff record according to staff ID only
   * @param p_stf_no: Staff No.
   *******************************************************************************************/
  procedure find_rec(p_stf_no in hr_staff.stf_no%type,p_msg varchar2:=null) as
    v_rec hr_staff%rowtype;
  begin
    if p_stf_no is null then
      raise no_data_found;
    end if;

    select * into v_rec
    from hr_staff
    where stf_no=p_stf_no;

    pkg_cookie.set_stfno(v_rec.stf_no);
    if not auth_vw_stf_dtls(v_rec.stf_no) then
      staff_partial_dtls(
        p_stf_no=>v_rec.stf_no,
        p_stf_hkid=>pkg_format.sh_partial_hkid(v_rec.stf_hkid),
        p_stf_sex=>v_rec.stf_sex
      );
      return;
    end if;
    staff_dtls (
      p_stf_no=>v_rec.stf_no,
      p_stf_name=>v_rec.stf_name,
      p_stf_cname=>v_rec.stf_cname,
      p_stf_hkid=>v_rec.stf_hkid,
      p_stf_pp_no=>v_rec.stf_pp_no,
      p_stf_pp_iscnty=>v_rec.stf_pp_iscnty,
      p_stf_nat=>v_rec.stf_nat,
      p_stf_dob=>v_rec.stf_dob,
      p_stf_sex=>v_rec.stf_sex,
      p_stf_marital_stat=>v_rec.stf_marital_stat,
      p_stf_addr1=>v_rec.stf_addr1,
      p_stf_addr2=>v_rec.stf_addr2,
      p_stf_addr3=>v_rec.stf_addr3,
      p_stf_addr4=>v_rec.stf_addr4,
      p_stf_addr_area=>v_rec.stf_addr_area,
      p_stf_ac_bnk_code=>v_rec.stf_ac_bnk_code,
      p_stf_ac_code=>v_rec.stf_ac_code,
      p_stf_sps_name=>v_rec.stf_sps_name,
      p_stf_sps_hkid=>v_rec.stf_sps_hkid,
      p_stf_sps_pp_no=>v_rec.stf_sps_pp_no,
      p_stf_sps_pp_iscnty=>v_rec.stf_sps_pp_iscnty,
      p_stf_sps_health=>v_rec.stf_sps_health,
      p_stf_dad_name=>v_rec.stf_dad_name,
      p_stf_dad_hkid=>v_rec.stf_dad_hkid,
      p_stf_dad_pp_no=>v_rec.stf_dad_pp_no,
      p_stf_dad_pp_iscnty=>v_rec.stf_dad_pp_iscnty,
      p_stf_dad_health=>v_rec.stf_dad_health,
      p_stf_mom_name=>v_rec.stf_mom_name,
      p_stf_mom_hkid=>v_rec.stf_mom_hkid,
      p_stf_mom_pp_no=>v_rec.stf_mom_pp_no,
      p_stf_mom_pp_iscnty=>v_rec.stf_mom_pp_iscnty,
      p_stf_mom_health=>v_rec.stf_mom_health,
      p_stf_empvisa_xdate=>v_rec.stf_empvisa_xdate,
      p_stf_permitno=>v_rec.stf_permitno,
      p_stf_permit_xdate=>v_rec.stf_permit_xdate,
      p_stf_actn=>v_rec.stf_actn,
      p_stf_actndate=>v_rec.stf_actndate,
      p_stf_actnuser=>v_rec.stf_actnuser,
      p_ts=>pkg_format.datetime2char(v_rec.timestamp),
      p_msg=>p_msg,
      ----------------------------------------------
      -- Angel Chan @ 20250219 added 
      p_stf_surname        => v_rec.stf_surname,
      p_stf_givenname      => v_rec.stf_givenname,
      p_stf_phone1areacode => v_rec.stf_phone1areacode,
      p_stf_email          => v_rec.stf_email,
      p_stf_phone1         => v_rec.stf_phone1  
      ----------------------------------------------
      
    );
  exception
    when no_data_found then
      pkg_cookie.set_stfno(null);
      staff_list (p_msg=>'No record found');
  end find_rec;

  /*******************************************************************************************
   * Find a staff record according to staff ID storing in a cookie in client's browser
   *******************************************************************************************/
  procedure find_rec_cookie as
  begin
    find_rec(p_stf_no=>pkg_cookie.get_stfno);
  end find_rec_cookie;


  /*******************************************************************************************
   * Check whether the specified HKID exists in staff record table
   * @param p_stf_hkid: Staff HKID No.
   * @return: true: HKID exists in our system, false: HKID does not exist in our system
   *******************************************************************************************/
  function isHKIDExists(p_stf_hkid in hr_staff.stf_hkid%type) return boolean
  is
    v_count pls_integer;
  begin
    select count(1) into v_count
    from hr_staff
    where trim(stf_hkid)=trim(p_stf_hkid);
    if v_count=1 then
      return true;
    end if;
    return false;
    -- chec whether the HKID exists
  end isHKIDExists;

  /*******************************************************************************************
   * Check whether the specified Passport No. exists in staff record table
   * @param p_stf_pp_no: Staff Passport No.
   * @return: true: Passport No. exists in our system,
   *          false: Passport No. does not exist in our system
   *******************************************************************************************/
  function isPP_NOExists(p_stf_pp_no in hr_staff.stf_pp_no%type) return boolean
  is
    v_count pls_integer;
  begin
    select count(1) into v_count
    from hr_staff
    where trim(stf_pp_no)=trim(p_stf_pp_no);
    if v_count>=1 then
      return true;
    end if;
    return false;
    -- check whether the passport number exists
  end;


  /*******************************************************************************************
   * Check whether the specified Staff No. exists in staff record table
   * @param p_stf_no: Staff No.
   * @return: true: Staff No. exists in our system,
   *          false: Staff No. does not exist in our system
   *******************************************************************************************/
  function isSTFNOExists(p_stf_no in hr_staff.stf_no%type) return boolean
  is
    v_count pls_integer;
  begin
    select count(1) into v_count
    from hr_staff
    where trim(stf_no)=trim(p_stf_no);
    if v_count=1 then
      return true;
    end if;
    return false;
  end isSTFNOExists;


end pkgf_staff;
