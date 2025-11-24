create or replace package body pkg_validate is
  ------------------------------------------------------------------------------------------------------------------------------
  -- Conrad Kwong @ 2016-10-19, Form# HRISPT-16006, Validate HKID check digit
  type char_arr is table of char(1) index by pls_integer;
  ------------------------------------------------------------------------------------------------------------------------------

  /***********************************************************************
   * Check whether there is an error message in the list of message
   * @param p_msg: list of message
   * @return true/false
   ***********************************************************************/
  function hasErrorMsg(p_msg pkg_rec.lst_rec_msg) return boolean as
  begin
    if p_msg is null or p_msg.count<=0 then
      return false;
    end if;
    for i in p_msg.first .. p_msg.count loop
      if p_msg(i).msg_type='E' then
        return true;
      end if;
    end loop;
    return false;
  exception
    when others then
      return true;
  end hasErrorMSg;

  /***********************************************************************
   * Check whether there is a warning message in the list of message
   * @param p_msg: list of message
   * @return true/false
   ***********************************************************************/
  function hasWarningMsg(p_msg pkg_rec.lst_rec_msg) return boolean as
  begin
    if p_msg is null or p_msg.count<=0 then
      return false;
    end if;
    for i in p_msg.first .. p_msg.count loop
      if p_msg(i).msg_type='W' then
        return true;
      end if;
    end loop;
    return false;
  exception
    when others then
      return true;
  end hasWarningMsg;

  /*****************************************************************************************
   * Check whether a staff record can be modified
   * Criteria of ability of modification:
   *  1. HR user can modify or
   *  2. SS user can modify or
   *  3. other users can modify if and only if the staff has no existing permanent contract
   * @param p_msg: list of message
   * @return true/false
   *****************************************************************************************/
  function canModifyStaff (p_stfno hr_staff.stf_no%type) return boolean as
  begin
    if (pkg_roleset.is_hrb_user) then
      return true;
    end if;
    return (pkg_info.has_perm_cntr(p_stfno)=false or pkg_roleset.is_ss_user);
  end;

  function valid_ctrtyp_ctrl_ac (
    p_ctr_code hr_centre.ctr_code%type,
    p_ctrl_ac  hr_ctr_typ.cty_ctrl_ac%type
  ) return boolean as
    v_rec hr_ctr_typ%rowtype;
    v_count binary_integer:=0;
    --Add by Raymond @ 20070514 : A/C Allocation Modification
    v_imc_code varchar2(2);
  begin
    -----------------------------------------------------------------------------
    -- Conrad Kwong @ 2018-10-04, Form# HRIS-19007, change for KG/NS centre
    /*if p_ctr_code is not null then

      --Add by Raymond @ 20070514 : A/C Allocation Modification
      select ctr_imc_code into v_imc_code
      from hr_centre
      where ctr_code = p_ctr_code;
      --------------------------------------------------------

      if v_imc_code is null then
        select * into v_rec
        from hr_ctr_typ
        where cty_code=pkg_info.get_ctr_typ(p_ctr_code);

        if v_rec.cty_ctrcat ='4010' or v_rec.cty_ctrcat = '4020' then
        return (v_rec.cty_ctrl_ac is not null and substr(v_rec.cty_ctrl_ac,1,3)=substr(p_ctrl_ac,1,3));
        else
        return (v_rec.cty_ctrl_ac is not null and substr(v_rec.cty_ctrl_ac,1,4)=substr(p_ctrl_ac,1,4));
        end if;

      --Add by Raymond @ 20070514 : A/C Allocation Modification
 \*     else
        select count(1) into v_count
        from hr_imc
        where imc_code = v_imc_code
              and p_ctrl_ac in (imc_bnk_ac,imc_mpfctrl_ac,imc_orsoctrl_ac);

        return (v_count>0);*\
      else
        return (substr(p_ctrl_ac,1,2) = v_imc_code);
      -----------------------------------------------------------
      end if;
    else
      select count(1) into v_count
      from hr_ctr_typ
      where substr(cty_ctrl_ac,1,4)=substr(p_ctrl_ac,1,4);
      return (v_count<=0);
    end if;

    return false;

    if need_chrg_ctr(p_ac_code => p_ctrl_ac) = 'Y' then
      return true;
    else
      return false;
    end if;*/
    return true;
  exception
    when no_data_found then
      return false;
  end valid_ctrtyp_ctrl_ac;

  /*------------------------------------------------------------*/
/*
  Author      : Felix Pang
  Created     : 17/2/2006
  Description : Funtion to validate the centre code with effective date, dormant date and deletion date
  Param Description:
  p_ctr_code : centre code for validation
  p_fr_date  : the from date is used to check against effective date of the centre
  p_to_date  : the to date is used to check against dormant date and deletion date of centre
  p_chk_type : this parameter has value either 'S' or 'C'.
               'S' is used for serving in the centre checking.
               'C' for charging to the centre checking.
               FSD allow user to input staff serve in a dummy cost centre, with effective date in 2099,
               but does not allow charging to such centre.

  Result:
   1: valid
  -1: error, no record found
  -2: error, centre not effective yet
  -3: warning, fr_date later than dormant date
  -4: error, fr_date later than deletion date (checking on this error has higher priority than '-3')
  -5: warning, to_date later than dormant date
  -6: error, to_date later than deletion date (checking on this error has higher priority than '-5')
  -7: error, centre should not be used for charging purpose.
  */
  function valid_ctr_code(p_ctr_code varchar2,
                          p_fr_date  date,
                          p_to_date  date,
                          p_chk_type char) return number is

    v_result number := null;

    cursor c_vaildate is
      select ctr_eff_date, ctr_dor_date, ctr_del_date
        from hr_centre
       where ctr_code = p_ctr_code;

  begin
    for v_record in c_vaildate loop

      -- checking from date
      -- early than effective date
      if p_fr_date < v_record.ctr_eff_date then
        if p_chk_type = 'S' then
          -- serving purpose
          if to_char(v_record.ctr_eff_date, 'YYYY') = 2099 then
            v_result := 1; -- valid
          else
            return - 2; -- not effective
          end if;
        elsif p_chk_type = 'C' then
          -- charging purpose
          if to_char(v_record.ctr_eff_date, 'YYYY') = 2099 then
            return - 7; -- not for charging
          else
            return - 2; -- not effective
          end if;
        end if;
      else
        -- later than effective(TRUE)
        v_result := 1;
      end if;
      -- checking from date with dormant, deletion date
      if p_fr_date >= v_record.ctr_del_date then
        return - 4; -- later than deletion date
      elsif p_fr_date >= v_record.ctr_dor_date then
        v_result := -3; -- later than dormant date
      else
        v_result := 1;
      end if;
      -- checking to date
      if to_char(v_record.ctr_eff_date, 'YYYY') != 2099 then
        -- don't need to check to date for dummy cost centre
        if p_to_date >= v_record.ctr_del_date then
          -- later than deletion date
          return - 6;
        elsif p_to_date >= v_record.ctr_dor_date then
          -- later than dormant date
          return - 5;
        end if;
      end if;
      return v_result; -- true
    end loop;

    -- if no record found
    return - 1;
  end valid_ctr_code;
/*------------------------------------------------------------*/

/*------------------------------------------------------------*/
  /*
    Author      : Raymond NG
    Created     : 17/5/2007
    Description : User cannot select IMC Chrg CTR / AC if the CNTR is under a non-imc ctr and vice versa
    Result:
      1 - Correct
      0 - CNTR uner IMC Centre but Chrg Centre is not or vice versa
  */
  function valid_chrg_ac(p_serv_ctr varchar2, p_chrg_ctr varchar2) return number is
     v_result number := 1;
  begin
    if is_imc_centre(p_serv_ctr) then
      if not is_imc_centre(p_chrg_ctr) then
         v_result := 0;
      end if;
    else
      if is_imc_centre(p_chrg_ctr) then
         v_result := 0;
      end if;
    end if;
    return v_result;
  end valid_chrg_ac;
/*------------------------------------------------------------*/

/*------------------------------------------------------------*/
  /*
    Author      : Raymond NG
    Created     : 17/5/2007
    Description : User cannot use a PF A/C for salary payment and vice versa
    Check Type  : 1 - Charge A/C, 2 - PF A/C
    Result:
      1 - Correct
      2 - Put the PF A/C as the normal Charge A/C
      3 - Put the normal Charge A/C as the PF A/C
  */
  function valid_ac_typ(p_chrg_ctr varchar2, p_chrg_ac varchar2, p_chk_typ number default 1) return number is
     v_result number := 1;
     v_count number := 0;
     v_imc_code varchar2(2);
  begin
     if substr(trim(p_chrg_ac),1,2) = '11' then
        return 1;
     end if;

     if p_chrg_ctr is not null then

       select ctr_imc_code into v_imc_code
       from hr_centre
       where ctr_code = p_chrg_ctr;
       -- Charge A/C
       if p_chk_typ = 1 then
          -- IMC CTR
          if v_imc_code is not null then
             select count(*) into v_count
             from hr_imcpopup_ac
             where ipa_ac_code = p_chrg_ac
                   and ipa_imc_code = v_imc_code;
             if v_count = 0 then
                v_result := 2;
             end if;
          -- Normal CTR
          else
              select count(*) into v_count
              from hr_ctrtyp_ac
              where cta_ac = p_chrg_ac;
              if v_count = 0 then
                 v_result := 2;
              end if;
          end if;

       -- PF A/C
       else
          -- IMC CTR
          if v_imc_code is not null then
             select count(*) into v_count
             from hr_imcpopup_pfac
             where ipp_pfac_code = p_chrg_ac
                   and ipp_imc_code = v_imc_code;
             if v_count = 0 then
                v_result := 3;
             end if;
          -- Normal CTR
          else
              select count(*) into v_count
              from hr_ctrtyp_pfac
              where ctpa_ac = p_chrg_ac;
              if v_count = 0 then
                 v_result := 3;
              end if;
          end if;
       end if;
     end if;
     return v_result;
  end valid_ac_typ;
/*------------------------------------------------------------*/


/*------------------------------------------------------------*/
  /*
    Author      : Felix Pang
    Created     : 17/2/2006
    Description : Funtion to validate the account code with effective date, dormant date and deletion date
  Param Description:
  p_acc_code : account code for validation
  p_fr_date  : the from date is used to check against effective date of the centre
  p_to_date  : the to date is used to check against dormant date and deletion date of account

  Result:
   1: valid
  -1: error, no record found
  -2: error, account not effective yet
  -3: warning, fr_date later than dormant date
  -4: error, fr_date later than deletion date (checking on this error has higher priority than '-3')
  -5: warning, to_date later than dormant date
  -6: error, to_date later than deletion date (checking on this error has higher priority than '-5')
  */
  function valid_acc_code(p_acc_code varchar2,
                          p_fr_date  date,
                          p_to_date  date) return number is

    v_result number := null;

    cursor c_vaildate is
      select acn_eff_date, acn_dor_date, acn_del_date
        from hr_ac_name
       where acn_ac_code = p_acc_code;
  begin

    for v_record in c_vaildate loop
      -- checking from date
      -- early than effective date
      if p_fr_date < v_record.acn_eff_date then
        return - 2; -- not effective
      else
        v_result := 1; -- effective
      end if;

      --checking from date with deletion date
      if p_fr_date > v_record.acn_del_date then
        return - 4; -- later than deletion date
      elsif p_fr_date > v_record.acn_dor_date then
        v_result := -3; -- later than dormant date
      else
        v_result := 1; -- true
      end if;

      -- checking to date
      if p_to_date >= v_record.acn_del_date then
        -- later than deletion date
        return - 6;
      elsif p_to_date >= v_record.acn_dor_date then
        -- later than dormant date
        return - 5;
      end if;
      return v_result; -- true
    end loop;

    -- if no record found
    return - 1;
  end valid_acc_code;
/*------------------------------------------------------------*/

  ------------------------------------------------------------------------------------------------------------------------------
  -- Conrad Kwong @ 2017-04-10, Form# HRISPT-17002, Validate Chinese Character
  --  reference: http://tomkuo139.blogspot.hk/2014/02/oracle-plsql.html
  function is_chinese(p_str varchar2) return char as
    v_count pls_integer := 0;
    v_result char(1) := 'N';
  begin
  select count(1)
    into v_count
    from dual
   where regexp_like(p_str, '[一-鶿]') or length(p_str) <> lengthb(p_str);

  if v_count > 0 then
    v_result := 'Y';
  else
    v_result := 'N';
  end if;

    return v_result;
  end;
  ------------------------------------------------------------------------------------------------------------------------------

  ------------------------------------------------------------------------------------------------------------------------------
  -- Conrad Kwong @ 2016-10-19, Form# HRISPT-16006, Validate HKID check digit
  function split_char(p_str varchar2) return char_arr as
    l_str varchar2(100) := upper(trim(p_str));
    l_arr char_arr;
  begin
    /*if length(l_str) = 0 then
      return l_arr;
    else*/
    if length(l_str) > 0 then
      for i in 1 .. length(l_str) loop
        l_arr(i) := substr(l_str,i,1);
      end loop;
    end if;
      return l_arr;
    --end if;
  end;

  function valid_hkid(p_hkid varchar2) return char as
    v_result      char(1) := 'N';
    v_hkid        varchar2(100) := upper(trim(p_hkid));
    v_hkid_wobk   varchar2(100) := replace(replace(v_hkid,'(',''),')','');
    v_hkid_arr    char_arr := split_char(v_hkid);
    v_hkid_chkdig char(1) := substr(v_hkid_wobk, length(v_hkid_wobk), 1);
    v_checksum      number;
    v_checkdigit    number;
    v_new_checkdigit char(1);
  begin
    if v_hkid_arr.count = 0 then
      return v_result;
    end if;

    -- 1234567890A
    -- AB123456(7)
    -- A123456(7)
    if length(v_hkid) < 10 then
      return v_result;
    end if;

    if (v_hkid_arr(8) != '(' and v_hkid_arr(9) != '(') or (v_hkid_arr(10) != ')' and v_hkid_arr(11) != ')') then
      return v_result;
    end if;

    -- convert to hkid char array without given chkdigit
    -- AB123456(A) --> AB123456A
    v_hkid_wobk := substr(v_hkid_wobk, 1, length(v_hkid_wobk) - 1);

    if not is_number(substr(v_hkid_wobk,length(v_hkid_wobk)-5,6)) then
      return v_result;
    end if;

    v_hkid_arr := split_char(v_hkid_wobk);

    v_checksum := 0;
    -------------------------------------------------------------------------------------
    -- http://hknothingblog.blogspot.hk/2013/01/javascript-to-validate-hkid-number.html
    if length(v_hkid_wobk) = 7 then
      v_checksum := v_checksum + (36 * 9);
      v_checksum := v_checksum + ((10 + ascii(v_hkid_arr(1)) - 65) * 8);
      --v_checksum := v_checksum + v_hkid_arr(2) * 7 + v_hkid_arr(3) * 6 + v_hkid_arr(4) * 5 + v_hkid_arr(5) * 4 + v_hkid_arr(6) * 3 + v_hkid_arr(7) * 2;
      v_checksum := v_checksum + to_number(v_hkid_arr(2)) * 7 + to_number(v_hkid_arr(3)) * 6 + to_number(v_hkid_arr(4)) * 5 + to_number(v_hkid_arr(5)) * 4 + to_number(v_hkid_arr(6)) * 3 + to_number(v_hkid_arr(7)) * 2;
    elsif length(v_hkid_wobk) = 8 then
      v_checksum := v_checksum + ((10 + ascii(v_hkid_arr(1)) - 65) * 9);
      v_checksum := v_checksum + ((10 + ascii(v_hkid_arr(2)) - 65) * 8);
      --v_checksum := v_checksum + v_hkid_arr(3) * 7 + v_hkid_arr(4) * 6 + v_hkid_arr(5) * 5 + v_hkid_arr(6) * 4 + v_hkid_arr(7) * 3 + v_hkid_arr(8) * 2;
      v_checksum := v_checksum + to_number(v_hkid_arr(3)) * 7 + to_number(v_hkid_arr(4)) * 6 + to_number(v_hkid_arr(5)) * 5 + to_number(v_hkid_arr(6)) * 4 + to_number(v_hkid_arr(7)) * 3 + to_number(v_hkid_arr(8)) * 2;
    else
      return v_result;
    end if;
    -------------------------------------------------------------------------------------
    v_checkdigit := 11 - mod(v_checksum, 11);

    if v_checkdigit = 10 then
      v_new_checkdigit := 'A';
    elsif v_checkdigit = 11 then
      v_new_checkdigit := '0';
    else
      v_new_checkdigit := to_char(v_checkdigit);
    end if;

    -- compare calculated check digit with given check digit
    if ascii(v_new_checkdigit) = ascii(v_hkid_chkdig) then
      v_result := 'Y';
    else
      v_result := 'N';
    end if;

    return v_result;

  exception
    when others then
      v_result := 'N';
      return v_result;

  end;
  ------------------------------------------------------------------------------------------------------------------------------

  ------------------------------------------------------------------------------------------------------------------------------
  -- Conrad Kwong @ 2021-07-30, Form# HRISPT-21003 : chrome adaption in Part-time
  --    prevent double submit
  procedure validate_duplicate(
    p_rec pkg_rec.rec_ptcntr,
    p_msg in out pkg_rec.lst_rec_msg
  ) as
    v_dup_cnt pls_integer := 0;
  begin
    if p_msg.count=0 then
      p_msg:=pkg_rec.lst_rec_msg();
    end if;

    select count(1)
      into v_dup_cnt
      from hr_ptcntr p
     where nvl(p.pct_stfno, '(NULL)') = nvl(p_rec.pct_stfno, '(NULL)')
       and nvl(p.pct_cntr_start, to_date('01/01/1900', 'dd/mm/yyyy')) = nvl(p_rec.pct_cntr_start, to_date('01/01/1900', 'dd/mm/yyyy'))
       and nvl(p.pct_cntr_end, to_date('01/01/1900', 'dd/mm/yyyy')) = nvl(p_rec.pct_cntr_end, to_date('01/01/1900', 'dd/mm/yyyy'))
       and nvl(p.pct_post_fract1, 99999) = nvl(p_rec.pct_post_fract1, 99999)
       and nvl(p.pct_post_fract2, 99999) = nvl(p_rec.pct_post_fract2, 99999)
       and nvl(trim(p.pct_postid), '(NULL)') = nvl(trim(p_rec.pct_postid), '(NULL)')
       and nvl(p.pct_cntr_post, '(NULL)') = nvl(p_rec.pct_cntr_post, '(NULL)')
       and nvl(p.pct_cntr_cpost, '(NULL)') = nvl(p_rec.pct_cntr_cpost, '(NULL)')
       and nvl(p.pct_std_post, '(NULL)') = nvl(p_rec.pct_std_post, '(NULL)')
       and nvl(p.pct_srv_ctr, get_user_dept) = nvl(p_rec.pct_srv_ctr, get_user_dept)
       and nvl(p.pct_mth_amt, -999) = nvl(p_rec.pct_mth_amt, -999)
       and nvl(p.pct_day_amt, -999) = nvl(p_rec.pct_day_amt, -999)
       and nvl(p.pct_hr_amt, -999) = nvl(p_rec.pct_hr_amt, -999)
       and nvl(p.pct_watchman, '(NULL)') = nvl(p_rec.pct_watchman, '(NULL)')
       and nvl(p.pct_remarks, '(NULL)') = nvl(p_rec.pct_remarks, '(NULL)')
       and nvl(p.pct_emp_reason, '(NULL)') = nvl(p_rec.pct_emp_reason, '(NULL)')
       and nvl(p.pct_supl_techr, '(NULL)') = nvl(p_rec.pct_supl_techr, '(NULL)')
       and nvl(p.pct_sub_techr, '(NULL)') = nvl(p_rec.pct_sub_techr, '(NULL)')
       --------------------------------------------------------------------------------------
       -- Conrad Kwong @ 2023-01-11, Form# HRISPT-22007, Enhancement for Imported Worker
       and nvl(p.pct_imp_wkr, '(NULL)') = nvl(p_rec.pct_imp_wkr, '(NULL)')
       --------------------------------------------------------------------------------------
       and nvl(p.pct_qualify, '(NULL)') = nvl(p_rec.pct_qualify, '(NULL)')
       and p.pct_entry_by = user
       and trunc(p.pct_entry_date,'dd') = trunc(sysdate,'dd')
       and nvl(p.pct_status, '(NULL)') in ('CE','SE')
       and p.pct_del_flg = 'N';

     if v_dup_cnt > 0 then
       p_msg.extend;
       p_msg(p_msg.count).msg_type:='E';
       p_msg(p_msg.count).msg:='Contract data duplicated.';
     end if;

     return;
  end;
  ------------------------------------------------------------------------------------------------------------------------------

  --------------------------------------------------------------------------------------
  -- Conrad Kwong @ 2023-01-11, Form# HRISPT-22007, Enhancement for Imported Worker
  procedure validate_impwrk(
    p_rec pkg_rec.rec_ptcntr,
    p_msg in out pkg_rec.lst_rec_msg
  ) as
    v_impwkr_cntr_flg char(1) := 'N';
  begin
    if p_msg.count=0 then
      p_msg:=pkg_rec.lst_rec_msg();
    end if;

    if is_csd_ctr(p_rec.pct_srv_ctr) then
      v_impwkr_cntr_flg := get_impwkr_cntr_flg(p_stfno    => p_rec.pct_stfno,
                                               p_fr_date  => p_rec.pct_cntr_start,
                                               p_cntr_ctr => p_rec.pct_cntr_ctr,
                                               p_cntr_yr  => p_rec.pct_cntr_yr,
                                               p_cntr_sqn => p_rec.pct_cntr_sqn);

      -- U = Without any pt contract by given date
      if v_impwkr_cntr_flg = 'U' then
        return;
      end if;

      -- E = Error case, both Y and N Imported Workder Flag exisst in current pt contracts by given date
      if v_impwkr_cntr_flg = 'E' then
          p_msg.extend;
          p_msg(p_msg.count).msg_type:='E';
          p_msg(p_msg.count).msg:='Having problem of get Imported Worker flag ('||p_rec.pct_imp_wkr||').';
      else
        if p_rec.pct_imp_wkr <> v_impwkr_cntr_flg then
          p_msg.extend;
          p_msg(p_msg.count).msg_type:='E';
          p_msg(p_msg.count).msg:='Imported Worker flag ('||p_rec.pct_imp_wkr||') is invalid.';
        end if;

        if p_rec.pct_imp_wkr = 'Y' and to_number(p_rec.pte_pf_schem) <> 0 then
          p_msg.extend;
          p_msg(p_msg.count).msg_type:='E';
          p_msg(p_msg.count).msg:='PFund Scheme should be zero while Imported Worker flag is Y.';
        end if;
      end if;
    else
      if p_rec.pct_imp_wkr <> 'N' then
        p_msg.extend;
        p_msg(p_msg.count).msg_type:='E';
        p_msg(p_msg.count).msg:='Imported Worker is for CSD centre only.';
      end if;
    end if;

    return;
  end;

  /***********************************************************************
   * Validate a staff record
   * @param p_mode: mode of access
   * @param p_rec: staff record
   * @return a list of message
   ***********************************************************************/
  function validation(p_mode varchar2:='insert',p_rec pkg_rec.rec_staff) return pkg_rec.lst_rec_msg as
    v_msg pkg_rec.lst_rec_msg;
    v_count pls_integer:=0;
  begin
    v_msg:=pkg_rec.lst_rec_msg();

    --------- If the staff has a permanent contract and the login user is not a secondary school user, --------
    --------- disallow to modify the record -------------------------------------------------------------------
    if p_rec.stf_no is not null and (not canModifyStaff(p_rec.stf_no)) then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='M';
      v_msg(v_msg.count).msg:='There is one or more contracts for this staff in the Human Resources Information System (Permanent).';
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='M';
      v_msg(v_msg.count).msg:='Please contact Human Resources Branch to modify the information of this staff if neccessary';
      return v_msg;
    end if;

    ------------- Not allow the staff number being empty during update -----------------
    if p_rec.stf_no is null and p_mode not like '%insert%' then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Staff No.] is a compulsory field';
    else
      select count(1) into v_count
      from hr_staff
      where stf_no=p_rec.stf_no;
      if p_mode='insert' and v_count >=1 then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='This [Staff No.] ('||p_rec.stf_no||') is already used in our staff records';
      end if;
    end if;

    ------------- Not allow the staff name being empty -------------------
    -- Angel Chan modofied @ 20250217 ---------------------------------------------------------
    if p_rec.stf_name is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Staff Name in English] is a compulsory field';
    end if;

    if p_rec.stf_surname is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Surname] is a compulsory field';
    end if;
    
    
    if length(p_rec.stf_name) > 75  then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='The combined length of surname, space, and given name should not be more than 75 characters.';
    end if;
    
    
    ------------------------------------------------------------------------------------------
    -- Conrad Kwong @ 2025-06-10, Form# HRIS-250xx, modify for eMPF
    --if (p_mode ='insert') then 
    if p_mode <> 'display' then 
      --if p_rec.stf_phone1areacode is null OR p_rec.stf_phone1 is null or p_rec.stf_email is null  then
      if p_rec.stf_phone1areacode is null OR p_rec.stf_phone1 is null then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='M';
        v_msg(v_msg.count).msg:='Mobile and Country Code are mandatory fields.';
      end if;
      -- Mobile number of the new employee. If it is a Hong Kong mobile no, 
      -- it must not start with '2' or '3'
      if substr(trim(to_char(p_rec.stf_phone1)),1,1) in ('2','3') then 
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='HK mobile number should not start with ‘2’ or ‘3’. Please input a valid mobile number.';
      end if;
    end if;
    ------------------------------------------------------------------------------------------

   -- Angel Chan modofied @ 20250217 ---------------------------------------------------------
  -------------------------------------------------------------------------------------------------------
  -- Conrad Kwong @ 2017-04-10, Form# HRISPT-17002, Validate Chinese Character
  if p_rec.stf_name is not null and is_chinese(p_str => p_rec.stf_name) = 'Y' then
    v_msg.extend;
    v_msg(v_msg.count).msg_type:='E';
    v_msg(v_msg.count).msg:='Chinese characters is not allowed in [Staff Name in English].';
  end if;

  if p_rec.stf_nat is not null and is_chinese(p_str => p_rec.stf_nat) = 'Y' then
    v_msg.extend;
    v_msg(v_msg.count).msg_type:='E';
    v_msg(v_msg.count).msg:='Chinese characters is not allowed in [Nationality].';
  end if;

  if p_rec.stf_pp_iscnty is not null and is_chinese(p_str => p_rec.stf_pp_iscnty) = 'Y' then
    v_msg.extend;
    v_msg(v_msg.count).msg_type:='E';
    v_msg(v_msg.count).msg:='Chinese characters is not allowed in [Passport Issue Country].';
  end if;

  if p_rec.stf_addr1 is not null and is_chinese(p_str => p_rec.stf_addr1) = 'Y' then
    v_msg.extend;
    v_msg(v_msg.count).msg_type:='E';
    v_msg(v_msg.count).msg:='Chinese characters is not allowed in [Address Line 1].';
  end if;
  if p_rec.stf_addr2 is not null and is_chinese(p_str => p_rec.stf_addr2) = 'Y' then
    v_msg.extend;
    v_msg(v_msg.count).msg_type:='E';
    v_msg(v_msg.count).msg:='Chinese characters is not allowed in [Address Line 2].';
  end if;
  if p_rec.stf_addr3 is not null and is_chinese(p_str => p_rec.stf_addr3) = 'Y' then
    v_msg.extend;
    v_msg(v_msg.count).msg_type:='E';
    v_msg(v_msg.count).msg:='Chinese characters is not allowed in [Address Line 3].';
  end if;
  if p_rec.stf_addr4 is not null and is_chinese(p_str => p_rec.stf_addr4) = 'Y' then
    v_msg.extend;
    v_msg(v_msg.count).msg_type:='E';
    v_msg(v_msg.count).msg:='Chinese characters is not allowed in [Address Line 4].';
  end if;

  if p_rec.stf_sps_name is not null and is_chinese(p_str => p_rec.stf_sps_name) = 'Y' then
    v_msg.extend;
    v_msg(v_msg.count).msg_type:='E';
    v_msg(v_msg.count).msg:='Chinese characters is not allowed in [Spouse Name].';
  end if;

  if p_rec.stf_dad_name is not null and is_chinese(p_str => p_rec.stf_dad_name) = 'Y' then
    v_msg.extend;
    v_msg(v_msg.count).msg_type:='E';
    v_msg(v_msg.count).msg:='Chinese characters is not allowed in [Father''s Name].';
  end if;

  if p_rec.stf_mom_name is not null and is_chinese(p_str => p_rec.stf_mom_name) = 'Y' then
    v_msg.extend;
    v_msg(v_msg.count).msg_type:='E';
    v_msg(v_msg.count).msg:='Chinese characters is not allowed in [Mother''s Name].';
  end if;
  -------------------------------------------------------------------------------------------------------

    ------------ Not allow input of duplicate Hong Kong Identification Card Number --------
    select count(1) into v_count
    from hr_staff
    where
      trim(stf_hkid)=nvl(trim(p_rec.stf_hkid),' ') and
      stf_no<>nvl(p_rec.stf_no,' ');

    if v_count>0 then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[HKID No.] There is another staff record using this HKID No.('||p_rec.stf_hkid||'). The system only allows one record for each staff. You may search the record of this staff by the HKID No.';
    end if;

    -----------------------------------------------------------------------------------------------------------------------------
    -- Conrad Kwong @ 2012-06-22, HRISPT-12004, Not allow input of duplicate HKID Card Number even in "Passport No." field --
    SELECT COUNT(1)
      INTO v_count
      FROM hr_staff
     WHERE TRIM(stf_pp_no) = nvl(TRIM(p_rec.stf_hkid), ' ')
       AND stf_no <> nvl(p_rec.stf_no, ' ');

    IF v_count > 0 THEN
      v_msg.extend;
      v_msg(v_msg.count).msg_type := 'E';
      v_msg(v_msg.count).msg := '[HKID No.] Someone already used this no. [' || p_rec.stf_hkid || '] as his/her HKID or Passport No. Please using staff''s HKID or Passport No.to find the staff record and check if it is the same person.';
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM hr_staff
     WHERE TRIM(stf_hkid) = nvl(TRIM(p_rec.stf_pp_no), ' ')
       AND stf_no <> nvl(p_rec.stf_no, ' ');

    IF v_count > 0 THEN
      v_msg.extend;
      v_msg(v_msg.count).msg_type := 'E';
      v_msg(v_msg.count).msg := '[Passport No.] Someone already used this no. [' || p_rec.stf_pp_no || '] as his/her HKID or Passport No. Please using staff''s HKID or Passport No.to find the staff record and check if it is the same person.';
    END IF;
    -----------------------------------------------------------------------------------------------------------------------------

    ------------------------------------------------------------------------------------------------------------------------------
    -- Conrad Kwong @ 2016-10-19, Form# HRISPT-16006, Validate HKID check digit
    IF lower(p_mode) <> 'display' AND TRIM(p_rec.stf_hkid) IS NOT NULL THEN
      IF valid_hkid(p_rec.stf_hkid) = 'N' THEN
        v_msg.extend;
        v_msg(v_msg.count).msg_type := 'M';
        v_msg(v_msg.count).msg := '[HKID No.] [' || trim(p_rec.stf_hkid) || '] is invalid. Please verify.';
      END IF;
    END IF;
    ------------------------------------------------------------------------------------------------------------------------------

    ----------------- Ensure either HKID No. or Passport No. is input -----------------
    if p_rec.stf_hkid is null and p_rec.stf_pp_no is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='Please input [HK ID No.] or/and [Passport No.]';
    end if;

    ---------------- Ensure the passport number is unique -----------------
    select count(1) into v_count
    from hr_staff
    where
      trim(stf_pp_no)=nvl(trim(p_rec.stf_pp_no),' ') and
      stf_no<>nvl(p_rec.stf_no,' ');
    if v_count>0 then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='This [Passport No.] There is another staff record using this Passport No.('||p_rec.stf_pp_no||'). The system only allows one record for each staff. You may search the record of this staff by the Passport No.';
    end if;

    ---------------- Not allow empty Date of Birth input -----------------
    if p_rec.stf_dob is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Date of Birth] is a compulsory field';
    --------------- Not allow date of birth being a future date -----------
    elsif months_between(sysdate,p_rec.stf_dob)/12<0 then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Date of Birth] cannot be a future date';
    --------------- Show warning if the staff is under 15 years old ------------------
    elsif months_between(sysdate,p_rec.stf_dob)/12<15 then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='M';
      v_msg(v_msg.count).msg:='This staff is under 15 years old';
    end if;

    ----------- Staff gender must be entered ------------------
    if p_rec.stf_sex is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Staff Sex] is a compulsory field';
    end if;
    ----------- Marital Status must be input ---------------
    if p_rec.stf_marital_stat is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Marital Status] is a compulsory field';
    end if;
    ----------- Address 1 must be filled -------------------
    if p_rec.stf_addr1 is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[First Line of Address] is a compulsory field';
    end if;
    if p_rec.stf_addr_area is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Address Area] is a compulsory field';
    end if;
    if p_rec.stf_ac_bnk_code is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Bank Account Code] is a compulsory field';
    else
      declare
        v_rec_bank hr_bank%rowtype;
      begin
        select * into v_rec_bank from hr_bank where bnk_code=p_rec.stf_ac_bnk_code;
      exception
        when no_data_found then
         v_msg.extend;
         v_msg(v_msg.count).msg_type:='E';
         v_msg(v_msg.count).msg:='[Bank Account Code] ('||p_rec.stf_ac_bnk_code||') is an invalid bank code';
      end;
    end if;
    if p_rec.stf_ac_code is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Account Code] is a compulsory field';
    elsif length(replace(trim(p_rec.stf_ac_code),'-',''))>12 and p_mode in ('insert','update') then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='Maximum length of [Account Code] is 12 excluding "-" sign';
    end if;
    if p_rec.stf_name is not null and not allow_chg_bnkac(p_rec.stf_no) then
      select count(1) into v_count
      from hr_staff
      where stf_no=p_rec.stf_no and trim(stf_name)<>trim(p_rec.stf_name);
      if v_count>0 then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='[Staff Name] cannot be changed Just after Payroll Process';
      end if;
    end if;
    if (p_rec.stf_ac_bnk_code is not null or p_rec.stf_ac_code is not null) and not allow_chg_bnkac(p_rec.stf_no) then
      select count(1) into v_count
      from hr_staff
      where
        stf_no=p_rec.stf_no and (
          trim(stf_ac_bnk_code)<>trim(p_rec.stf_ac_bnk_code) or
          trim(stf_ac_code)<>trim(p_rec.stf_ac_code)
        );
      if v_count>0 then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='[Bank Account Code] and [Account Code] cannot be changed Just after Payroll Process';
      end if;
    end if;
    if (p_rec.stf_pp_no is not null and p_rec.stf_pp_iscnty is null) or
       (p_rec.stf_pp_no is null and p_rec.stf_pp_iscnty is not null) then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Passport No.] and [Issue Country] must be both filled or both empty';
    end if;
    if nvl(p_rec.stf_marital_stat,' ')='M' and p_rec.stf_sps_name is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Spouse Name] is compulsory if [marital status] is set to "Married". Please enter [spouse name] or set [marital status] to "Married without spouse ID".';
    end if;
    if (p_rec.stf_permitno is not null and p_rec.stf_permit_xdate is null) or
       (p_rec.stf_permitno is null and p_rec.stf_permit_xdate is not null) then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Permit Number] and [Permit Expiry Date] must be both filled in or must be left them empty.';
    end if;

    ------------ Show warning about modification of staff ------------------
    select count(1) into v_count
    from hr_staff inner join hr_ptcntr on stf_no=pct_stfno
    where
      stf_no=p_rec.stf_no and
      pct_del_flg='N' and
      pct_cntr_ctr<>get_user_dept;
    if v_count>0 then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='M';
      v_msg(v_msg.count).msg:='Warning: This staff has contract(s) in other centres. Data changes of this staff will also affect them.';
    end if;

    return v_msg;
  end validation;


  /***************************************************************************************
   * Validate a contract record can be terminated
   * Criteria of termination:
   *  1. the contract is on or after payroll process
   *  2. check whether there is a disconnection of contract periods
   *  3. desired contract end date must be earlier than the last payroll contract end date
   * @param p_rec: contract record
   * @param p_msg: output of record message
   ***************************************************************************************/
  procedure validate_termination (
    p_rec pkg_rec.rec_ptcntr,
    p_msg in out pkg_rec.lst_rec_msg
  ) as
    v_last_pyrl_cntr_end_date hr_pt_emp.pte_lastpyrl_cntr_enddate%type;
    v_ptpaym_period rec_period;
    v_count number;
  begin
    if p_msg.count=0 then
      p_msg:=pkg_rec.lst_rec_msg();
    end if;


    --------------- Checking whether the contract is payroll processed -----------------
    --------------- The contract must be on/after payroll processed --------------------
    if p_rec.pct_status<>pkg_tx_status.pyrl_cmpl then
      p_msg.extend;
      p_msg(p_msg.count).msg_type:='E';
      p_msg(p_msg.count).msg:='The Contract has not payroll processed';
    end if;

    ------------------ Check whether there is disconnection of periods -------------
    ------------------ Disconnection of periods is not allowed ---------------------
    declare
      v_req_pf boolean; -- for dummy use
    begin
      validate_emp(p_rec,p_msg,v_req_pf);
    end;

    ------------------ Check whether the desired contract end date -------------------------------
    --- The desired contract end date must be earlier than the last payroll contract end date ----
    /**************************
    v_last_pyrl_cntr_end_date:=pkg_emp.get_last_pyrl_cntrend_date(p_rec.pct_empno);
    if p_rec.pct_cntr_end>v_last_pyrl_cntr_end_date then
      p_msg.extend;
      p_msg(p_msg.count).msg_type:='E';
      p_msg(p_msg.count).msg:='The Contract End date must be earlier than the last payroll contract end date, ' || to_char(v_last_pyrl_cntr_end_date,'dd/mm/yyyy');
    end if;
    **************************/

    ----------------------- Check against the payment period range ----------------------
    ----------- The desired contract end date must be later than the largest payment

    --Modify by Raymond @ 20071231: Need to also consider payment records at pt payment history table
    /*v_ptpaym_period:=pkg_info.get_ptpaym_period_range(p_rec.pct_cntr_ctr,p_rec.pct_cntr_yr,p_rec.pct_cntr_sqn);
    if v_ptpaym_period.to_date>p_rec.pct_cntr_end then
      p_msg.extend;
      p_msg(p_msg.count).msg_type:='E';
      p_msg(p_msg.count).msg:='The Contract End date must be on/after the last payment to date '|| to_char(v_ptpaym_period.to_date,'dd/mm/yyyy');
    end if;*/
    select count(*) into v_count from
     (
     select ppt_cntr_ctr, ppt_cntr_yr, ppt_cntr_sqn
     from hr_ptpaym_tx
     where ppt_cntr_ctr = p_rec.pct_cntr_ctr
           and ppt_cntr_yr = p_rec.pct_cntr_yr
           and ppt_cntr_sqn = p_rec.pct_cntr_sqn
           and ppt_revert = 'N'
           and ppt_del_flg = 'N'
           and ppt_to_date > p_rec.pct_cntr_end
     union
     select ppth_cntr_ctr, ppth_cntr_yr, ppth_cntr_sqn
     from hr_ptpaym_tx_his
     where ppth_cntr_ctr = p_rec.pct_cntr_ctr
           and ppth_cntr_yr = p_rec.pct_cntr_yr
           and ppth_cntr_sqn = p_rec.pct_cntr_sqn
           and ppth_revert = 'N'
           and ppth_del_flg = 'N'
           and ppth_to_date > p_rec.pct_cntr_end
     );

     if v_count > 0 then
      p_msg.extend;
      p_msg(p_msg.count).msg_type:='E';
      p_msg(p_msg.count).msg:='The Contract End date must be on/after the last payment to date';
     end if;
    -----------------------------------------------------------------------------------------------
  end validate_termination;

  /***********************************************************************
   * Append employment notice
   * @param p_rec: staff record
   * @param p_msg: output list of message
   ***********************************************************************/
  procedure append_emp_notice (p_rec pkg_rec.rec_ptcntr, p_msg in out pkg_rec.lst_rec_msg) as
    v_count binary_integer:=0;
  begin
    --------------- If this contract is the first contract of an employment record ---------
    select count(1) into v_count
    from hr_pt_emp
    where
      pte_emp_no=p_rec.pct_empno and
      to_number(pte_pf_schem)=6 and
      pte_crtr_cntr_ctr=p_rec.pct_cntr_ctr and
      pte_crtr_cntr_yr=p_rec.pct_cntr_yr and
      pte_crtr_cntr_sqn=p_rec.pct_cntr_sqn;
    if v_count>=1 then
      p_msg.extend;
      p_msg(p_msg.count).msg_type:='M';
      --Steve @30-03-2011 updated IN51 to IN61
      p_msg(p_msg.count).msg:='This is the first contract of this employment. Please submit IN61 for the staff.';
    end if;
  end append_emp_notice;

  /***********************************************************************
   * Validate a contract record
   * @param p_mode: mode of access
   * @param p_rec: contract record
   * @return a list of message
   ***********************************************************************/
  function validation(p_mode varchar2:='insert', p_rec pkg_rec.rec_ptcntr) return pkg_rec.lst_rec_msg as
    v_msg pkg_rec.lst_rec_msg;
    v_count pls_integer:=0;
    update_count pls_integer:=0;
    v_valid_code number;
    v_rec hr_ptcntr%rowtype;
    v_log hr_ptcntr_log%rowtype;

    cursor c_chk is
        select * into v_log
        from hr_ptcntr_log
        where pctl_actn = 'UPDATE'
              and (instr(pctl_chg_desc,'Contract Start') > 0 or instr(pctl_chg_desc,'Contract End') > 0)
              and pctl_cntr_ctr = p_rec.pct_cntr_ctr
              and pctl_cntr_yr = p_rec.pct_cntr_yr
              and pctl_cntr_sqn = p_rec.pct_cntr_sqn
        order by pctl_actn_date desc;

    ------------------------------------------------------------------
    -- Angel Chan added @ 20250220
    v_valid_enrol_empf  varchar(1);
    v_exist_staff     varchar(1);
    ------------------------------------------------------------------
  begin
    v_msg:=pkg_rec.lst_rec_msg();

    ------------ Add by Raymond @ 20071214 -------------------------
       /*select * into v_rec
       from hr_ptcntr
       where pct_cntr_ctr = p_rec.pct_cntr_ctr
             and pct_cntr_yr = p_rec.pct_cntr_yr
             and pct_cntr_sqn = p_rec.pct_cntr_sqn;

       if v_rec.pct_cntr_start <> p_rec.pct_cntr_start or v_rec.pct_cntr_start <> p_rec.pct_cntr_start then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          v_msg(v_msg.count).msg:='[Error]You cannot modify the contract period when there is an approved payment';
       end if;

       if v_rec.pct_srv_ctr <> p_rec.pct_srv_ctr then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          v_msg(v_msg.count).msg:='[Error]You cannot modify the serving centre when there is an approved payment';
       end if;

       if v_rec.pct_mth_amt <> p_rec.pct_mth_amt or v_rec.pct_day_amt <> p_rec.pct_day_amt or v_rec.pct_hr_amt <> p_rec.pct_hr_amt then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          v_msg(v_msg.count).msg:='[Error]You cannot modify the paid rate when there is an approved payment';
       end if;   */
    ----------------------------------------------------------------

    ------------ If it is marked as deleted, do not valid -----------
    if p_rec.pct_del_flg='Y' then
      return v_msg;
    end if;
    -----------------------------------------------------------------

    ------------------------------------------------------------------------------------------------------------------------------
    -- Conrad Kwong @ 2021-07-30, Form# HRISPT-21003 : chrome adaption in Part-time
    --    prevent double submit
    if p_mode like '%insert%' then
      validate_duplicate(p_rec, v_msg);
      if v_msg.count > 0 then
        return v_msg;
      end if;
    end if;
    ------------------------------------------------------------------------------------------------------------------------------

    --------------------------------------------------------------------------------------
    -- Conrad Kwong @ 2023-01-11, Form# HRISPT-22007, Enhancement for Imported Worker
    validate_impwrk(p_rec, v_msg);
    --------------------------------------------------------------------------------------

    ------------ For termination validation ---------
    if p_rec.pct_status=pkg_tx_status.pyrl_cmpl then
      validate_termination (p_rec,v_msg);
    end if;
    -------------------------------------------------

    ---------------------------------------- Checking of Contract No. ---------------------------------------
    if (p_rec.pct_cntr_ctr is null or p_rec.pct_cntr_yr is null or p_rec.pct_cntr_sqn is null ) and
       p_mode not like '%insert%' then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Contract No.] is a compulsory field';
    else
      select sum(counts) into v_count
      from (
        select count(1) counts
        from hr_ptcntr
        where pct_cntr_ctr=p_rec.pct_cntr_ctr and pct_cntr_yr=p_rec.pct_cntr_yr and pct_cntr_sqn=p_rec.pct_cntr_sqn
        union
        select count(1) counts
        from hr_ptcntr_del
        where pctd_cntr_ctr=p_rec.pct_cntr_ctr and pctd_cntr_yr=p_rec.pct_cntr_yr and pctd_cntr_sqn=p_rec.pct_cntr_sqn
      );
      if p_mode like '%insert%' and v_count >=1 then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='This [Contract No.] ('||pkg_format.contract(p_rec.pct_cntr_ctr,p_rec.pct_cntr_yr,p_rec.pct_cntr_sqn)||') is already used in our system';
      end if;
    end if;
    ------------------------------------ Checking of Staff No. -----------------------------------------------
    if p_rec.pct_stfno is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Staff No.] is a compulsory field';
    else
      select count(1) into v_count
      from hr_staff
      where stf_no=p_rec.pct_stfno;
      if v_count <=0 then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='[Staff No.] ('||p_rec.pct_stfno||') does not exist in our system';
      end if;
    end if;
    ------------------------------------ Checking of Serving Centre -----------------------------------------------
    if p_rec.pct_srv_ctr is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Serving Centre] is a compulsory field';
    else
     if valid_ctr_code(p_rec.pct_srv_ctr,p_rec.pct_cntr_start,p_rec.pct_cntr_end,'S')!=1 then
        v_valid_code :=valid_ctr_code(p_rec.pct_srv_ctr,p_rec.pct_cntr_start,p_rec.pct_cntr_end,'S');
        if v_valid_code!=1 then
            if v_valid_code = -1 then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='[Serving Centre] does not exist.';
            elsif v_valid_code = -2 then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='[Serving Centre] is not effective yet.';
            elsif v_valid_code = -3 then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='W';
              v_msg(v_msg.count).msg:='From date is later than serving centre dormant date.';
            elsif v_valid_code = -4 then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='From date is later than serving centre deletion date.';
            elsif v_valid_code = -5 then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='W';
              v_msg(v_msg.count).msg:='To date is later than serving centre dormant date.';
            elsif v_valid_code = -6 then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='To date is later than serving centre deletion date.';
            elsif v_valid_code = -7 then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='[Serving Centre] should not be used for charging purpose.';
            else
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='Invaild centre of [Serving Centre].';
            end if;
          end if;

        elsif get_user_dept<>p_rec.pct_srv_ctr and not is_subordinate_ctr(p_rec.pct_srv_ctr) and
            not pkg_roleset.is_hrb_user and not pkg_roleset.is_fsd_user then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='[Serving Centre] ('||p_rec.pct_srv_ctr||') is neither your centre ('||get_user_dept||') nor your subordinate centre';
      else
        -------------------------------------- Checking using pre-IMC centre code
        -- Appended by Freeman Ng @ 20060207


        select count(1) into v_count
        from hr_sch2imc_ctrcnv
        where sch_ctr_code=p_rec.pct_srv_ctr;
        if p_mode like '%insert%' and v_count >=1 then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          v_msg(v_msg.count).msg:='This is a pre-IMC school centre code. Please use IMC centre username to create contract.';
        end if;
        -------------------------------------- End Checking using pre-IMC centre code


        -------------------------------------- Checking on those contract should be created in day nursery centre after Mar 2006
        -- Appended by Freeman Ng @ 20060306
        if p_mode like '%insert%' then
           if p_rec.pct_srv_ctr in ('504827',
                                     '504832',
                                     '504848',
                                     '504853',
                                     '504869',
                                     '504874',
                                     '504880',
                                     '504895',
                                     '504902',
                                     '504918',
                                     '504923',
                                     '504939',
                                     '504950',
                                     '504965') then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              --v_msg(v_msg.count).msg:='No contract should be created in day nursery centre after Mar 2006. Please contact Miu Wong, Tel: 28597714, or Jo Chan, Tel: 28597713, for details.';
              v_msg(v_msg.count).msg:='Creation / Renewal of contracts under Day Nursery is not allowed. Please create / renew contracts under Nursery School or Integrated program. If you have any queries, please contact Ms. Jo Chan at 28597713 or Ms. Miu Wong at 28597714.';
           -------------------------------------------------------------------------------------------   
           -- Conrad Kwong @ 2025-05-27, Form# HRIS-250xx, 
           --        Centre Code re-use, allow create PT contract for centre code 502219 & 502224
           /*elsif p_rec.pct_srv_ctr in ('502219', '502224') then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='Creation / Renewal of contracts is not allowed.';*/
           -------------------------------------------------------------------------------------------   
           end if;


        end if;
        -------------------------------------- End Checking on those contract should be created in day nursery centre after Mar 2006

      end if;
    end if;
    --------------------------------- Checking of Contract Start Date ------------------------------
    if p_rec.pct_cntr_start is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Contract Start Date] is a compulsory field';
    end if;
    --------------------------------- Checking of Contract End Date --------------------------------
    if p_rec.pct_cntr_end is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Contract End Date] is a compulsory field';
    else
      if (p_mode like '%insert%' or p_mode like '%update') and
         to_char(p_rec.pct_cntr_end,'mmdd')<>to_char(pkg_info.get_fin_yrend_date,'mmdd') and
         to_char(p_rec.pct_cntr_end,'mmdd')<>to_char(pkg_info.get_sch_yrend_date,'mmdd') and
         to_char(p_rec.pct_cntr_end,'yyyymmdd') not in ('20041231','20051231') then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='M';
        v_msg(v_msg.count).msg:='[Contract End Date] should be either '|| last_day(trunc(pkg_info.get_fin_yrend_date,'mm')) || ' or ' || last_day(trunc(pkg_info.get_sch_yrend_date,'mm'));
      end if;
    end if;

    ------------------------ Checking of Contract Start Date and End Date --------------------------
    if p_rec.pct_cntr_start is not null and p_rec.pct_cntr_end is not null then
      if p_rec.pct_cntr_start>=p_rec.pct_cntr_end then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='[Contract Start Date] must be earlier than [Contract End Date]';
      else
        -- warn when it's an "expired" contract
        if (p_mode like '%insert%' or p_mode like '%update') and p_rec.pct_cntr_end < pkg_info.get_pyrl_mth then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='M';
          v_msg(v_msg.count).msg:='The [Contract End Date] of this contract is earlier than the start of current payroll month, are you sure to create/modify this contract?';
        end if;
        -- over 14 months or 12 months
        if months_between(p_rec.pct_cntr_end,p_rec.pct_cntr_start)>14 then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          v_msg(v_msg.count).msg:='Contract period is over 12 month duration.';
        elsif months_between(p_rec.pct_cntr_end,p_rec.pct_cntr_start)>12 then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='M';
          v_msg(v_msg.count).msg:='Reminder : Contract period is over 12 month duration.';
        end if;
      end if;
    end if;

    if p_rec.pct_cntr_start is not null and
       p_rec.pct_cntr_end is not null and
       (pkg_info.get_yrend_date-p_rec.pct_cntr_start+1)<60 and
       (p_rec.pct_cntr_end-p_rec.pct_cntr_start+1)<60 then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='M';
        v_msg(v_msg.count).msg:='The contract period of this contract is less than 60 days. You may set [Contract End Date] to the next financial/school year end for contract to enroll in MPF contribution';
    end if;

    --------------------------------- Checking of Contract Post ------------------------------------
    if p_rec.pct_cntr_post is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Contract Post in English] is a compulsory field';
    end if;
    -------------------------------- Checking of Post ID -------------------------------------------
    if p_rec.pct_postid is not null then
      select sum(counting) into v_count
      from (
             select count(*) counting
             from
             hr_csd_post csd_post,
             (select csdp_postid,max(csdp_effdate) csdp_effdate from hr_csd_post
              where csdp_postid = p_rec.pct_postid and
                    csdp_effdate<=p_rec.pct_cntr_start group by csdp_postid
             ) max_csd_post
             where
             csd_post.csdp_postid=p_rec.pct_postid and
             csd_post.csdp_postid=max_csd_post.csdp_postid and
             csd_post.csdp_effdate=max_csd_Post.csdp_effdate
             union
             select count(*) counting
             from hr_ed_post edp_post,
             (select edp_postid,max(edp_effdate) edp_effdate from hr_ed_post
              where edp_postid = p_rec.pct_postid and
                    edp_effdate<=p_rec.pct_cntr_start group by edp_postid
             ) max_edp_post
             where
             edp_post.edp_postid=p_rec.pct_postid and
             edp_post.edp_postid=max_edp_post.edp_postid and
             edp_post.edp_effdate=max_edp_Post.edp_effdate
             ----------------------------------------------------------------------------------
             -- Conrad Kwong @ 2021-07-30, Form# HRISPT-21003 : chrome adaption in Part-time --
             union
             select count(*) counting
             from
             hr_csdse_post csdse_post,
             (select csep_postid,max(csep_effdate) csep_effdate from hr_csdse_post s
              where csep_postid = p_rec.pct_postid and
                    csep_effdate<=p_rec.pct_cntr_start group by csep_postid
             ) max_csdse_post
             where
             csdse_post.csep_postid=p_rec.pct_postid and
             csdse_post.csep_postid=max_csdse_post.csep_postid and
             csdse_post.csep_effdate=max_csdse_post.csep_effdate
             ----------------------------------------------------------------------------------
            );
      if v_count<=0 then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='M';
        v_msg(v_msg.count).msg:='[Contract Post ID] not found';
      end if;
    end if;
    ------------------------------- Checking of Standard Post ------------------------------------
    if upper(nvl(p_rec.pct_std_post,' ')) not in ('Y','N') then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Standard Post ?] must be set to either [Yes] or [No]';
    end if;
    ------------------------------- Checking of Post Fraction ------------------------------------
    if p_rec.pct_post_fract1 is null or p_rec.pct_post_fract2 is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Post Fraction] is a compulsory field';
    end if;
    ------------------------- Checking of Denominator of Post Fraction -------------------------
    if nvl(p_rec.pct_post_fract2,0)=0 then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='Denominator of [Post Fraction] cannot be zero';
    end if;
    ------------------------------- Checking of Pay Rate ------------------------------------
    if p_rec.pct_mth_amt is null and p_rec.pct_day_amt is null and p_rec.pct_hr_amt is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='At least One of [Month Amount], [Daily Amount] and [Hourly Amount] must be filled in';
    end if;
    ------------------------------- Checking of [is watchman post] --------------------------
    if upper(nvl(trim(p_rec.pct_watchman),' ')) not in ('Y','N') then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Watchman Post ?] is a compulsory field and must has a [Yes] or [No] value';
    end if;
    ------------------------If [is watchman post] is Yes, check permit expiration--------------------------
    if upper(nvl(trim(p_rec.pct_watchman),' '))='Y' and p_mode like '%display%' then
      declare
        v_permit_xdate hr_staff.stf_permit_xdate%type;
      begin
        select stf_permit_xdate into v_permit_xdate from hr_staff
        where stf_no=p_rec.pct_stfno;
        if v_permit_xdate is null then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          v_msg(v_msg.count).msg:='[Watchman Post ?] cannot be set to [Yes] unless the [Permit No.] and [Permit Expiry Date] in Staff Particulars are filled in.';
        elsif v_permit_xdate<p_rec.pct_cntr_start then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          v_msg(v_msg.count).msg:='[Watchman Post ?] cannot be set to [Yes] because the [Permit Expiry Date] in Staff Particulars has expired';
        end if;
      exception
        when no_data_found then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          v_msg(v_msg.count).msg:='Error in retrieving the permit expiry date of staff (Staff No: '|| p_rec.pct_stfno ||') during contract creation/update';
        when too_many_rows then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          v_msg(v_msg.count).msg:='Error in retrieving the permit expiry date of staff (Staff No: '|| p_rec.pct_stfno ||') during contract creation/update';
       end;
    end if;
    ------------------------------- Checking of [is supply teacher] --------------------------
    if upper(nvl(trim(p_rec.pct_supl_techr),' ')) not in ('Y','N') then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Supply Teacher ?] is a compulsory field and must has a [Yes] or [No] value';
    end if;
    ------------------------------- Checking of [is substitute teacher] --------------------------
    if upper(nvl(trim(p_rec.pct_sub_techr),' ')) not in ('Y','N') then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Substititue Teacher ?] is a compulsory field and must has a [Yes] or [No] value';
    end if;
    --------------------------------------------------------------------------------------
    -- Conrad Kwong @ 2023-01-11, Form# HRISPT-22007, Enhancement for Imported Worker
    ------------------------------- Checking of [is Imported Worker] --------------------------
    if upper(nvl(trim(p_rec.pct_imp_wkr),' ')) not in ('Y','N') then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Imported Worker ?] is a compulsory field and must has a [Yes] or [No] value';
    end if;
    --------------------------------------------------------------------------------------
    ------------------------------- Checking of [Employment Reason] --------------------------
    if p_rec.pct_emp_reason is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Employment Reason] is a compulsory field';
    end if;
    ------------------------------ Checking of [is teacher] & [is watchman] -------------------------------
    --------------------------------------------------------------------------------------
    -- Conrad Kwong @ 2023-01-11, Form# HRISPT-22007, Enhancement for Imported Worker
    --if (upper(p_rec.pct_supl_techr)='Y' or upper(p_rec.pct_sub_techr)='Y') and
    if (upper(p_rec.pct_supl_techr)='Y' or upper(p_rec.pct_sub_techr)='Y' or upper(p_rec.pct_imp_wkr)='Y') and
    --------------------------------------------------------------------------------------
       upper(p_rec.pct_watchman)='Y' then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='It cannot be a [Watchman] post and a [Teacher] post in the same contract';
    end if;
    ------------------------ Checking for PF related fields ------------------------------------------
    select count(1) into v_count
    from hr_pt_emp
    where pte_stfno=p_rec.pct_stfno and pte_active_cntr=0;
    if v_count>=1 then
      ------------------------------- Checking of Provident Fund Scheme --------------------------
      if nvl(p_rec.pte_pf_schem,-1) not in (0,6,7,8) then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='[Provident Fund Scheme] is invalid';
      end if;
      ---------------------- Checking of MPF Lower Bound Employee Contribution Flag -----------------------
      if v_count=1 and upper(nvl(trim(p_rec.pte_mpf_lb_eec_flg),' ')) not in ('Y','N') then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='[MPF Lower Bound Employee Contribution] is a compulsory field and must has a [Yes] or [No] value';
      end if;
    end if;
    ---------------------- Checking of Staff Qualified with Proof ------------------------------------
    if upper(nvl(trim(p_rec.pct_qualify),' '))<>'Y' then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Staff Qualified with Proof] is a compulsory field and must be checked';
    end if;

  -------------------------------------------------------------------------------------------------------
  -- Conrad Kwong @ 2017-04-10, Form# HRISPT-17002, Validate Chinese Character
  if p_rec.pct_cntr_post is not null and is_chinese(p_str => p_rec.pct_cntr_post) = 'Y' then
    v_msg.extend;
    v_msg(v_msg.count).msg_type:='E';
    v_msg(v_msg.count).msg:='Chinese characters is not allowed in [Post in English].';
  end if;

  if p_rec.pct_remarks is not null and is_chinese(p_str => p_rec.pct_remarks) = 'Y' then
    v_msg.extend;
    v_msg(v_msg.count).msg_type:='E';
    v_msg(v_msg.count).msg:='Chinese characters is not allowed in [Remarks].';
  end if;
  -------------------------------------------------------------------------------------------------------

    ---------------------- Check MPF related data ----------------------------------------------------
    -----------------------------------------------------------------------------------------------------------------
    -- Conrad Kwong @ 2014-10-30, Form# HRISPT-14004, bugfix for case whom age under 18 and payroll completed
    if nvl(p_rec.pct_status, '@@') <> pkg_tx_status.pyrl_cmpl then
    -----------------------------------------------------------------------------------------------------------------
      if p_rec.pct_cntr_start<pkg_mpf.get_age_date(p_rec.pct_stfno,18) and to_number(p_rec.pte_pf_schem) not in (0,7) then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='[P. Fund Scheme] must be Nil or Under 18 as the staff is under 18 years old on the date of contract starts';
      elsif p_rec.pct_cntr_start>=pkg_mpf.get_age_date(p_rec.pct_stfno,65) and to_number(p_rec.pte_pf_schem) not in (0,8) then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='[P. Fund Scheme] must be Nil or Over 65 as the staff is over 65 years old on the date of contract starts';
      -----------------------------------------------------------------------------------------------------------------------------------------------
      -- Conrad Kwong @ 2015-11-12, Form# HRISPT-15002, bugfix, should check the contract start date under age 65 rather then between age 18 & 65
      --elsif p_rec.pct_cntr_start between pkg_mpf.get_age_date(p_rec.pct_stfno,18) and pkg_mpf.get_age_date(p_rec.pct_stfno,65) and
      elsif p_rec.pct_cntr_start >= pkg_mpf.get_age_date(p_rec.pct_stfno,18) and p_rec.pct_cntr_start < pkg_mpf.get_age_date(p_rec.pct_stfno,65) and
            to_number(p_rec.pte_pf_schem) not in (0,6) then
      -----------------------------------------------------------------------------------------------------------------------------------------------
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
      -----------------------------------------------------------------------------------------------------------------------------------------------
      -- Conrad Kwong @ 2015-11-12, Form# HRISPT-15002, bugfix, should check the contract start date under age 65 rather then between age 18 & 65
      --    change the error message wording to match with the logic
        --v_msg(v_msg.count).msg:='[P. Fund Scheme] must be Nil or MPF as the staff is between 18 and 65 years old on the date of contract starts';
        v_msg(v_msg.count).msg:='[P. Fund Scheme] must be Nil or MPF as the staff is equal to/greater than 18 and under 65 years old on the date of contract starts';
      -----------------------------------------------------------------------------------------------------------------------------------------------
      end if;
    -----------------------------------------------------------------------------------------------------------------
    -- Conrad Kwong @ 2014-10-30, Form# HRISPT-14004, bugfix for case whom age under 18 and payroll completed
    end if;
    -----------------------------------------------------------------------------------------------------------------

    -----------------------------------------------------------------------------------------------------------------
    ---------------------- Check EMPF related data  ----------------------------------------------------
    -- Angel Chan added @ 20250220
    ------------------------------
    -- To check whether the staff has email / phone number 
    SELECT COUNT(1)
    INTO v_valid_enrol_empf
    FROM HR_STAFF h
    WHERE h.stf_no = p_rec.pct_stfno
       ------------------------------------------------------------------------------------------
       -- Conrad Kwong @ 2025-06-10, Form# HRIS-250xx, modify for eMPF
       --AND h.stf_email is not null
       ------------------------------------------------------------------------------------------
       AND h.stf_phone1 is not null
       AND h.stf_phone1areacode is not null;


    -- To check whether the staff is a existing mpf member, if yes > then continue to use 
    -- else , the staff will be input as a new member 
    /*
    SELECT COUNT(1)
    INTO v_exist_staff
    FROM HR_PTCNTR pt
    WHERE pt.pct_stfno = p_rec.pct_stfno
      AND pt.pct_srv_ctr = p_rec.pct_srv_ctr
      AND sysdate < (SELECT max(pt2.pct_cntr_end)
                     FROM HR_PTCNTR pt2
                     WHERE PCT_DEL_FLG = 'N'
                       AND PCT_STATUS = 'PC'
                       AND  pt2.pct_stfno = p_rec.pct_stfno)
      AND pt.pct_empno = (
          SELECT wk.pew_emp_no  
          FROM hr_pt_emp_wk wk
          WHERE wk.pew_terminate != 'Y' --> existing people 
            AND wk.pew_pf_schem in ('6')
      ); */
      
     -- To check whether it is existing in the parttime contract  and they are MPF 
     -- else anyone who create need to check 
     SELECT COUNT(1)
     INTO v_exist_staff 
     FROM hr_pt_emp emp
     WHERE emp.pte_terminate = 'N' 
       AND to_number(emp.pte_pf_schem) = 6
       AND emp.pte_stfno = p_rec.pct_stfno 
       AND (emp.pte_emp_no IN ( SELECT DISTINCT(pt.pct_empno)
                               FROM HR_PTCNTR pt
                               WHERE pt.pct_stfno = p_rec.pct_stfno 
                                 AND pt.pct_del_flg = 'N'
                                 AND pt.pct_status = 'PC'
                                 -- for active emp no  
                                 AND emp.pte_emp_no = pt.pct_empno
                                 --AND pt.pct_srv_ctr = p_rec.pct_srv_ctr  >> still TW group >> no need for enrolment again in TW group 
                                 -- but need enrolment from another entity 
                                 AND (
                                      -- For TW group 
                                        ( 
                                          is_imc_centre_yn(p_rec.pct_srv_ctr) = 'N' 
                                          AND 
                                          is_kgns_centre_yn(p_rec.pct_srv_ctr) = 'N'
                                          AND
                                          is_lglnty_ctr(p_rec.pct_srv_ctr) = 'N'
                                        )  
                                      OR 
                                      -- For different IMC, legal entity, kgns ctr group
                                      -- different code is different independant entity 
                                      pt.pct_srv_ctr = p_rec.pct_srv_ctr
                                   ) 
                               )
             -- To prevent the contract enter before 01/06/2025 will also show the warning 
             -- as we assume that 01/06/2025 eMPF will be online                       
             -- OR emp.pte_firstjoin_date < to_date('01/06/2025','dd/MM/yyyy')
         ); 
                              
       
           --> existing mpf people
           --> '6' : MPF ; '7' : < 18 years ; '8' : > 65 years ; 0 : 'exempted' 
           --> 7 is 18 years old  --> 18 yrs old should be need to enrol in MPF 
      

    -- check whether exist in the hr_ptcntr ,
    -- then check whether this person has work in the centre that input,
    -- then check the cntr_end_date whether it is exist
    -- if all yes >>> '1' then exist staff
    -- if one is no >>> '0' new enrol

    IF ((p_mode like '%insert%' OR p_mode like '%update%') AND /*v_exist_staff = 0 AND*/ v_valid_enrol_empf = 0 AND to_number(p_rec.pte_pf_schem) in (6)) THEN
       v_msg.extend;
       v_msg(v_msg.count).msg_type:='M';
       ------------------------------------------------------------------------------------------
       -- Conrad Kwong @ 2025-06-10, Form# HRIS-250xx, modify for eMPF
       --v_msg(v_msg.count).msg:='[Personal Particulars] Both Staff Personal Email Address and Staff Mobile Phone Number are required for eMPF enrolment. You can fill in the information after the contract creation';
       v_msg(v_msg.count).msg:='[Personal Particulars] Mobile and Country Code are mandatory fields.';
       ------------------------------------------------------------------------------------------
    END IF;
    
    IF ( p_mode like '%display%' AND v_exist_staff = 0 AND v_valid_enrol_empf = 0 AND to_number(p_rec.pte_pf_schem)in (6)) THEN
     ------------------------------------------------------------------------------------------------------------------------------
     -- To check whether the contract is terminated >> if yes , then no need to show the error in the display page    
     SELECT COUNT(1)
     INTO v_exist_staff
     FROM  HR_PTCNTR pt
     WHERE pt.pct_cntr_ctr = p_rec.pct_cntr_ctr
       AND pt.pct_cntr_yr  = p_rec.pct_cntr_yr 
       AND pt.pct_cntr_sqn = p_rec.pct_cntr_sqn 
       AND pt.pct_empno IN (
          SELECT emp.pte_emp_no
          FROM hr_pt_emp emp
          WHERE emp.pte_terminate = 'Y' 
            AND emp.pte_pf_schem in ('6')
            AND emp.pte_stfno = p_rec.pct_stfno
      ); 
     ---------------------------------------------------------------------------------------------------------------------------------
      IF(v_exist_staff = 0) THEN 
         v_msg.extend;
         v_msg(v_msg.count).msg_type:='E';
         ------------------------------------------------------------------------------------------
         -- Conrad Kwong @ 2025-06-10, Form# HRIS-250xx, modify for eMPF
         --v_msg(v_msg.count).msg:='[Personal Particulars] Both Staff Personal Email Address and Staff Mobile Phone Number are required for eMPF enrolment.';
         v_msg(v_msg.count).msg:='[Personal Particulars] Mobile and Country Code are mandatory fields.';
         ------------------------------------------------------------------------------------------
      END IF;
    END IF;

    ------------------------------------------------------------------------------------------------------
    ---------------------------------------------------------------------------------------------------------------------------------------------
    -- Conrad Kwong @ 2008-05-08, Bugfix,
    --    show error message only when the MPF contribution flag (pte_mpf_lb_eec_flg)
    --    of part time employment table is not equal to current form pte_mpf_lb_eec_flg
/*
    if p_mode like '%update%' and
       (not pkg_roleset.is_supervisor) and
       p_rec.pte_mpf_lb_eec_flg='Y' then
*/
    if p_mode like '%update%' and
       (not pkg_roleset.is_supervisor) and
       p_rec.pct_empno is not null and
       p_rec.pte_mpf_lb_eec_flg is not null then
      declare
        v_mpf_lb_eec_flg hr_pt_emp.pte_mpf_lb_eec_flg%type := NULL;
      begin
        select pte_mpf_lb_eec_flg
          into v_mpf_lb_eec_flg
          from hr_pt_emp
         where pte_emp_no = p_rec.pct_empno;

        if nvl(v_mpf_lb_eec_flg, 'N') <> p_rec.pte_mpf_lb_eec_flg then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          --v_msg(v_msg.count).msg:='Only Supervisor user can set the item [MPF contribution, if income below '||pkg_format.money(pkg_mpf.get_mpf_lb_amt)||' to [Yes]';
          v_msg(v_msg.count).msg:='Only Supervisor user can modify the item [MPF contribution, if income below '||pkg_format.money(pkg_mpf.get_mpf_lb_amt)||']';
        end if;
      exception
        when no_data_found then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          v_msg(v_msg.count).msg:='Contract ' || p_rec.pct_cntr_ctr || '-' || p_rec.pct_cntr_yr || '-' || p_rec.pct_cntr_sqn || ' record not found in Part Time Employment Table!';
      end;
    end if;
    ---------------------------------------------------------------------------------------------------------------------------------------------

    ---------------------- Check Employment related data ---------------------------------------------
    declare
      v_req_pf boolean; -- for dummy use
    begin
      validate_emp(p_rec,v_msg,v_req_pf);
    end;

    --------------------- Check Contract and Period related data -------------------------------------
    if p_mode like '%display%' then
      validate_cntr_paym(p_rec,v_msg);  -- check contract and period related data
      -- check whether the contract start date is after the end of next payroll month
      if p_rec.pct_cntr_start>last_day(add_months(pkg_info.get_pyrl_mth,1)) then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='You are not allowed to approve this contract due to [Contract Start Date] is later than next payroll month month end ('||to_char(last_day(add_months(pkg_info.get_pyrl_mth,1)),'dd/mm/yyyy')||'). You may approve this contract later.';
      end if;
    end if;
    --------------------------------------------------------------------------------------------------

    append_emp_notice(p_rec,v_msg);
    return v_msg;
  exception
    when others then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:=sqlerrm;
      return v_msg;
  end validation;

  /***********************************************************************
   * Validate data about contract-payment related
   * @param p_rec: contract record
   * @param p_msg: output of list of message
   ***********************************************************************/
  procedure validate_cntr_paym (p_rec pkg_rec.rec_ptcntr, p_msg in out pkg_rec.lst_rec_msg) as
    v_paym_period rec_period;
    cursor c is
      select * from hr_ptpaym_tx
      where
        ppt_cntr_ctr = p_rec.pct_cntr_ctr and
        ppt_cntr_yr = p_rec.pct_cntr_yr and
        ppt_cntr_sqn = p_rec.pct_cntr_sqn;
    v_is_payrate_changed boolean;
  begin
    if p_msg.count=0 then
      p_msg:=pkg_rec.lst_rec_msg();
    end if;
    if nvl(pkg_tx_status.get_ptcntr_nxt_status(p_rec.pct_status),' ') not in (pkg_tx_status.sch_aprv,pkg_tx_status.ctr_aprv) then
      return;
    end if;

    ------------- Check whether the contract period exceeds the period rand of its related payments -----------
    v_paym_period:=pkg_info.get_ptpaym_period_range(p_rec.pct_cntr_ctr,p_rec.pct_cntr_yr,p_rec.pct_cntr_sqn);
    if v_paym_period.fr_date not between p_rec.pct_cntr_start and p_rec.pct_cntr_end or
       v_paym_period.to_date not between p_rec.pct_cntr_start and p_rec.pct_cntr_end then
      p_msg.extend;
      p_msg(p_msg.count).msg_type:='E';
      p_msg(p_msg.count).msg:='The period range of all related payment transactions ('||v_paym_period.fr_date||' to '||v_paym_period.to_date||') exceeds the contract period.';
    end if;

    ---------- Check for each corresponding payment of the contract --------------------------------------------------
    ---------- See whether there is unmatch case of contract pay rate amount and payment pay rate amount -------------
    for item in c loop
      v_is_payrate_changed:=false;
      if item.ppt_pay_rate=pkg_info.MONTHLY_RATE and item.ppt_pay_rate_amt<> nvl(p_rec.pct_mth_amt,0) then
        v_is_payrate_changed:=true;
      elsif item.ppt_pay_rate=pkg_info.DAILY_RATE and item.ppt_pay_rate_amt<> nvl(p_rec.pct_day_amt,0) then
        v_is_payrate_changed:=true;
      elsif item.ppt_pay_rate=pkg_info.HOURLY_RATE and item.ppt_pay_rate_amt<> nvl(p_rec.pct_hr_amt,0) then
        v_is_payrate_changed:=true;
      end if;
    end loop;
    if v_is_payrate_changed then
      p_msg.extend;
      p_msg(p_msg.count).msg_type:='E';
      p_msg(p_msg.count).msg:='This contract cannot be approved because the its pay rate has been changed after creation of the corresponding payments.';
      p_msg.extend;
      p_msg(p_msg.count).msg_type:='E';
      p_msg(p_msg.count).msg:='Please modify its corresponding payment(s) before this contract approval.';
    end if;

    return;
  exception
    when others then
      p_msg.extend;
      p_msg(p_msg.count).msg_type:='E';
      p_msg(p_msg.count).msg:=sqlerrm;
  end validate_cntr_paym;

  /***********************************************************************
   * Check whether the contract allows input of P.Fund data
   * @param p_rec: contract record
   * @return true/false
   ***********************************************************************/
  function require_pf_data(p_rec pkg_rec.rec_ptcntr) return boolean as
    v_req_pf boolean;
    v_msg pkg_rec.lst_rec_msg;
  begin
    v_msg:=pkg_rec.lst_rec_msg();
    validate_emp (p_rec,v_msg,v_req_pf);
    return v_req_pf;
  end require_pf_data;

  /***********************************************************************
   * Check whether the contract has P. Fund data input
   * @param p_rec: contract record
   * @return true/false
   ***********************************************************************/
  function has_pf_data(p_rec pkg_rec.rec_ptcntr) return boolean as
  begin
    return (
       p_rec.pct_cntr_start is not null and
       p_rec.pte_pf_schem is not null and
       p_rec.pte_mpf_lb_eec_flg is not null
    );
  end has_pf_data;

  /***********************************************************************
   * Validate an employment record
   * @param p_rec: contract record
   * @param p_msg: output of list of message
   * @param p_req_pf: output parameter -- requires P. Fund data input?
   ***********************************************************************/
  procedure validate_emp (
    p_rec pkg_rec.rec_ptcntr,
    p_msg in out pkg_rec.lst_rec_msg,
    p_req_pf out boolean
  ) as
    v_rec_emp hr_pt_emp%rowtype;
    v_new_period rec_period;
    v_count binary_integer:=0;
    v_emp_end date;
    /*26 8 2005 , variable for part time terminate */
    v_ov_flg char(1);
    cursor c_emp is
    select pte_firstjoin_date, pte_cess_date,pte_emp_no from hr_pt_emp where
    pte_terminate='Y' and
    pte_stfno=p_rec.pct_stfno;
    v_overlap char(1):='N';
    -----------------------------------
    -----------------------------------------------------------------------------------------
    -- Conrad Kwong @ 2012-04-02, Form# HRISPT-11004,
    --        also check continuous contract while latest contract's termination flag is Y
    v_diff_emp char(1) := 'N';
    -----------------------------------------------------------------------------------------
  begin
    if p_msg.count=0 then
      p_msg:=pkg_rec.lst_rec_msg();
    end if;
    p_req_pf:=false;
    ---------------------------------------------------------------------------------------
    -- Conrad Kwong @ 2025-06-10, Form# HRIS-250xx, modify for eMPF
    --        include serving centre for legal entity checking
    --v_rec_emp:=pkg_emp.get_cur_emp(p_rec.pct_stfno);
    v_rec_emp:=pkg_emp.get_cur_emp(p_stfno=>p_rec.pct_stfno, p_srv_ctr=>p_rec.pct_srv_ctr);
    ---------------------------------------------------------------------------------------

    v_new_period:=rec_period(p_rec.pct_cntr_start,p_rec.pct_cntr_end);

    -----------------------------------------------------------------
    -- no employment record is found --> it is a new employment
    -- no PF data is supplied.
    -- In this case, PF data must be provided
    -----------------------------------------------------------------------------------------
    -- Conrad Kwong @ 2012-04-02, Form# HRISPT-11004,
    --------------------------------------------------------------------------------------------------
    -- Conrad Kwong @ 2012-04-02, Form# HRISPT-11013, bugfix, NULL emp no. returned while employment terminated
    --      Only check the PF data in case of employemnt not termintated
    --      Assume that no overlap period for new employment
    /*
    if v_rec_emp.pte_emp_no is null and not has_pf_data(p_rec) then
      p_msg.extend;
      p_msg(p_msg.count).msg_type:='E';
      p_msg(p_msg.count).msg:='PF data is required';
      p_req_pf:=true;
    end if;
    */
    --------------------------------------------------------------------------------------------------
    ----------------------------------------------------------------

    if has_pf_data(p_rec) and
       to_number(p_rec.pte_pf_schem)<>6 and
       nvl(p_rec.pte_mpf_lb_eec_flg,'Y')='Y' then
      p_msg.extend;
      p_msg(p_msg.count).msg_type:='E';
      p_msg(p_msg.count).msg:='[MPF Lower Bound Employee Contribution] must be [No] if the P. Fund Scheme is not MPF';
    end if;



    -----------------------------------------------------------------------------
    -- PART TIME Termination
    begin
    --select max(pte_cess_date) into v_emp_end from hr_pt_emp where
    --pte_stfno=p_rec.pct_stfno and
    --pte_terminate= 'Y';

    -----------------------------------------------------------------------------------------
    -- Conrad Kwong @ 2022-10-07, Form# HRISPT-220xx, bugfix,
    --        should get the flag of latest employment
    -- over ride date back contract modifitcation @ 26 8 2005
    /*select max(PTE_DATBAK_CNTR_OVRRIDE) into v_ov_flg  from hr_pt_emp where
    pte_stfno=p_rec.pct_stfno and
    pte_terminate = 'Y';
    --group by PTE_DATBAK_CNTR_OVRRIDE ;*/
    select pte_datbak_cntr_ovrride
      into v_ov_flg
      from hr_pt_emp e
     where e.pte_stfno = p_rec.pct_stfno
       and e.pte_terminate = 'Y'
       and e.pte_emp_no = (select max(p.pte_emp_no)
                             from hr_pt_emp p
                            where p.pte_stfno = e.pte_stfno
                              and p.pte_terminate = e.pte_terminate);
    -----------------------------------------------------------------------------------------

    -----------------------------------------------------------------
    -- no employment record is found --> it is a new employment
    -- no PF data is supplied.
    -- In this case, PF data must be provided
    --------------------------------------------------------------------------------------------------
    -----------------------------------------------------------------------------------------
    -- Conrad Kwong @ 2012-04-02, Form# HRISPT-11004,
    -----------------------------------------------------------------------------------------
    -- Conrad Kwong @ 2012-04-02, Form# HRISPT-11013, bugfix, NULL emp no. returned while employment terminated
    --      Only check the PF data in case of employemnt not termintated
    --      Assume that no overlap period for new employment
      if v_overlap = 'N' then
        if v_rec_emp.pte_emp_no is null and not has_pf_data(p_rec) then
          p_msg.extend;
          p_msg(p_msg.count).msg_type:='E';
          p_msg(p_msg.count).msg:='PF data is required';
          p_req_pf:=true;
        end if;
      end if;
    --------------------------------------------------------------------------------------------------

    for item in c_emp loop
      -----------------------------------------------------------------------------------------
      -- Conrad Kwong @ 2012-04-02, Form# HRISPT-11004,
      --        also check continuous contract while latest contract's termination flag is Y
      if nvl(item.pte_emp_no,-1) <> nvl(p_rec.pct_empno,-1) then
        v_diff_emp := 'Y';
      end if;
      -----------------------------------------------------------------------------------------
    if is_ctu_period(fr1=> p_rec.pct_cntr_start, to1=>p_rec.pct_cntr_end , fr2=> item.pte_firstjoin_date, to2=> item.pte_cess_date) = true then
    v_overlap:='Y';
    end if;
    end loop;

    exception
       when no_data_found then
         null;
    end;


    --if v_emp_end is not null and p_rec.pct_cntr_start <= v_emp_end +1 then
    -- over ride date back contract modifitcation @ 26 8 2005
    -----------------------------------------------------------------------------------------
    -- Conrad Kwong @ 2012-04-02, Form# HRISPT-11004,
    --        also check continuous contract while latest contract's termination flag is Y
    --if v_overlap='Y' and v_ov_flg ='N' and p_rec.pct_empno is null then
    if v_overlap='Y' and v_ov_flg ='N' and (p_rec.pct_empno is null or v_diff_emp = 'Y') then
  --------------------------------------------------------------------------------------
    p_msg.extend;
    p_msg(p_msg.count).msg_type:='E';
    ----------------------------------------------------------------------------------------
    -- Conrad Kwong @ 2021-07-30, Form# HRISPT-21003 : chrome adaption in Part-time
    --p_msg(p_msg.count).msg:='This contract period (start/end dateo) constitutes'||
    p_msg(p_msg.count).msg:='This contract period (start/end date) constitutes'||
    ----------------------------------------------------------------------------------------
                            ' a continuous employment with a terminated employment '||
                            'of the staff. If you are sure this is correct, please '||
                            'contact Finance & Supplies Division.';
    end if;
    -----------------------------------------------------------------------------


    -----------------------------------------------------------------------------
    -- if employment period and input period are not connected --> return error
    begin
      select count(1) into v_count
      from hr_pt_emp
      where pte_emp_no=nvl(p_rec.pct_empno,pte_emp_no) and
            pte_terminate='N';
    exception
      when no_data_found then
        null;
    end;
    if v_count>0 and not pkg_emp.is_period_connect (v_rec_emp.pte_emp_no,v_new_period,p_rec.pct_cntr_ctr,p_rec.pct_cntr_yr,p_rec.pct_cntr_sqn) then
      p_msg.extend;
      p_msg(p_msg.count).msg_type:='E';
      p_msg(p_msg.count).msg:='The period of [contract start date] and [contract end date] must connect to the period of [first appointment date] ('||to_char(v_rec_emp.PTE_FIRSTJOIN_DATE,'dd/mm/yyyy')||') and [termination date] ('||to_char(v_rec_emp.PTE_CESS_DATE,'dd/mm/yyyy')||')';
    end if;
    -----------------------------------------------------------------------------

    ---------------------------------------------------------------------------------------------------
    -- The remaining code indicates that the employment period and the input period connect each other

    if v_rec_emp.pte_lastpyrl_cntr_enddate is not null and
       to_char(v_new_period.fr_date,'yyyymm')=to_char(v_rec_emp.pte_lastpyrl_cntr_enddate,'yyyymm') and
       v_new_period.fr_date-1 > v_rec_emp.pte_lastpyrl_cntr_enddate then
      p_msg.extend;
      p_msg(p_msg.count).msg_type:='E';
      p_msg(p_msg.count).msg:='The [contract start date] must be the next date of the [contract end date of the last payroll], '|| to_char(v_rec_emp.pte_lastpyrl_cntr_enddate) || ' of the employment record';
    end if;
    --------------------------------------------------------------------------------------------------

  end validate_emp;

  -----------------------------------------------------------------------------
  -- Conrad Kwong @ 2018-10-04, Form# HRIS-19007, change for KG/NS centre
  function kgns_ac_chk(p_serv_ctr hr_centre.ctr_code%type, p_chrg_ctr hr_centre.ctr_code%type, p_ac varchar2) return boolean is
   v_result boolean := true;
   v_imc_code hr_centre.ctr_imc_code%type := null;
  begin
    if p_chrg_ctr is not null then
      if is_kgns_centre_yn(p_chrg_ctr) = 'Y' then
        select ctr_imc_code into v_imc_code
          from hr_centre
         where ctr_code = p_chrg_ctr;

        if substr(p_ac,1,2) != v_imc_code then
          v_result := false;
        end if;
      elsif is_imc_centre(p_chrg_ctr) then
        /*select ctr_imc_code into v_imc_code
          from hr_centre
         where ctr_code = p_chrg_ctr;

        if substr(p_ac,1,2) != '11' and substr(p_ac,1,2) != v_imc_code then
          v_result := false;
        end if;*/
        if is_kgns_centre_yn(p_serv_ctr) = 'Y' then
          v_result := false;
        end if;
      else
        if not is_number(substr(p_ac,1,2)) then
          v_result := false;
        end if;
      end if;
    end if;

    return v_result;
  end;

  FUNCTION kgns_ctrlac_chk(p_serv_ctr hr_centre.ctr_code%TYPE,
                          p_chrg_ctr hr_centre.ctr_code%TYPE,
                          p_ac       VARCHAR2) RETURN BOOLEAN IS
    v_result   BOOLEAN := TRUE;
    v_inc_ctl  hr_centre.ctr_inc_ctl%TYPE;
    v_exp_ctl  hr_centre.ctr_exp_ctl%TYPE;
  BEGIN
    -----------------------------------------------------------------------------
    -- Conrad Kwong @ 2018-10-04, Form# HRIS-19007, change for KG/NS centre
    /*BEGIN
      SELECT ctr_inc_ctl, ctr_exp_ctl
        INTO v_inc_ctl, v_exp_ctl
        FROM hr_centre
       WHERE ctr_code = p_serv_ctr;

      IF substr(p_ac, 1, 4) IN (substr(v_inc_ctl, 1, 4), substr(v_exp_ctl, 1, 4)) AND p_chrg_ctr IS NULL THEN
        v_result := FALSE;
      END IF;

    EXCEPTION
      WHEN OTHERS THEN
        v_result := FALSE;
    END;*/
    IF need_chrgctr(p_ac => p_ac) = 'Y' AND p_chrg_ctr IS NULL THEN
       v_result := FALSE;
    ELSE
       v_result := TRUE;
    END IF;

    RETURN v_result;
  END;
  -----------------------------------------------------------------------------


  /***********************************************************************
   * Validate a payment record
   * @param p_mode: mode of access
   * @param p_rec: payment record
   * @return a list of message
   ***********************************************************************/
  function validation(p_mode varchar2:='insert', p_rec pkg_rec.rec_ptpaym) return pkg_rec.lst_rec_msg as
  /******************************************************
   * Validation of Payment record.
   * Use in payment_form, payment_dtls, pkgf_payment
   ******************************************************/
    v_msg          pkg_rec.lst_rec_msg;
    v_count        pls_integer := 0;
    v_gross_amt    number := 0;                          -- gross amount of payment
    v_chrg_total   number := 0;                          -- total allocate amount of all charge centre
    v_cntr_start   hr_ptcntr.pct_cntr_end%type;          -- start date of the contract
    v_cntr_end     hr_ptcntr.pct_cntr_end%type;          -- end date of the contract
    v_cntr_status  hr_ptcntr.pct_status%type;            -- status of the contract
    v_permit_xdate hr_staff.stf_permit_xdate%type;       -- staff's permit expiry date
    v_pyrl_mth     date := pkg_info.get_pyrl_mth;
    v_fin_yr_start date := pkg_info.get_fin_yrstart_date(v_pyrl_mth);
    v_valid_code number;
  begin
    v_msg:=pkg_rec.lst_rec_msg();

    ------------ If it is marked as deleted, do not valid -----------
    if p_rec.ppt_del_flg='Y' then
      return v_msg;
    end if;
    -----------------------------------------------------------------

    /* ------------------------------ Checking of Reference No. ------------------------------ */
    if p_rec.ppt_refno is null and p_mode not like '%insert%' then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Reference No.] is a mandatory field';
    else
      select sum(counts) into v_count
      from (
        select count(1) counts
        from hr_ptpaym_tx
        where ppt_refno=p_rec.ppt_refno
        union
        select count(1) counts
        from hr_ptpaym_tx_del
        where pptd_refno=p_rec.ppt_refno
      );
      if p_mode like '%insert%' and v_count >=1 then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='The [Reference No.] ('|| p_rec.ppt_refno ||') is already used in our system';
      end if;
    end if;

    /* ------------------------------ Checking of Status ------------------------------ */
    select pct_status into v_cntr_status
    from hr_ptcntr
    where pct_cntr_ctr = p_rec.ppt_cntr_ctr
      and pct_cntr_yr  = p_rec.ppt_cntr_yr
      and pct_cntr_sqn = p_rec.ppt_cntr_sqn;
    -- compare status between contract and payment
    if (p_rec.ppt_status in (pkg_tx_status.ctr_entry, pkg_tx_status.ctr_aprv)
        and v_cntr_status in (pkg_tx_status.sch_entry, pkg_tx_status.sch_aprv)) or
       (p_rec.ppt_status in (pkg_tx_status.sch_entry, pkg_tx_status.sch_aprv)
        and v_cntr_status in (pkg_tx_status.ctr_entry, pkg_tx_status.ctr_aprv))
    then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='mis-match in payment status and corresponding contract status, this will lead to different process flow of transactions.';
    end if;
    /* ------------------------------ Checking of From Date ------------------------------ */
    if p_rec.ppt_fr_date is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Period From Date] is a mandatory field';
    /* ------------ Checking Watchman Permit Expiry Date against Payment To Date ------------ */
    elsif pkg_info.is_watchman_permit_xdate(p_rec.ppt_cntr_ctr, p_rec.ppt_cntr_yr, p_rec.ppt_cntr_sqn, v_permit_xdate) then
      if p_rec.ppt_fr_date >= v_permit_xdate or v_permit_xdate is null then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='W';
        v_msg(v_msg.count).msg:='Warning: [Period From Date] is out of staff''s permit expiry date ('||v_permit_xdate||')';
      end if;
    end if;
    /* ------------------------------ Checking of To Date ------------------------------ */
    if p_rec.ppt_to_date is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Period To Date] is a mandatory field';
    end if;
    /* ------------------------------ Checking of From Date/To Date period ------------------------------ */
    if p_rec.ppt_fr_date is not null and p_rec.ppt_to_date is not null then
      if p_rec.ppt_fr_date>p_rec.ppt_to_date then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='Payment period [To Date] must be on or after [From Date]';
      end if;

      -- get contract start date and end date
      select pct_cntr_start, pct_cntr_end
      into v_cntr_start, v_cntr_end
      from hr_ptcntr
      where pct_cntr_ctr = p_rec.ppt_cntr_ctr
      and pct_cntr_yr = p_rec.ppt_cntr_yr
      and pct_cntr_sqn = p_rec.ppt_cntr_sqn;
      -- payment period must be within contract serve period
      if p_rec.ppt_fr_date < v_cntr_start then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='Payment period [From Date] is out of contract period (Contract Period: '||v_cntr_start||' to '||v_cntr_end||')';
      end if;
      if p_rec.ppt_to_date > v_cntr_end then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='Payment period [To Date] is out of contract period (Contract Period: '||v_cntr_start||' to '||v_cntr_end||')';
      end if;
      -- Post dated checking
      if not
         ((pkg_pt_pyrlproc_ctrl.get_batch_no = 0 and p_rec.ppt_to_date <= last_day(v_pyrl_mth))
          or (pkg_pt_pyrlproc_ctrl.get_batch_no > 0 and p_rec.ppt_to_date <= last_day(add_months(v_pyrl_mth,1)))
          or (p_rec.ppt_to_date <= last_day(sysdate)))
      then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='Payment period cannot be post dated.';
      end if;
      -- Backpay payment and period checking
      -- Period must be earlier than curent financial year start when Backpay payment type
      /*
      if (p_rec.ppt_paym_code = 'A011P' and
          (p_rec.ppt_fr_date >= v_fin_yr_start or
           p_rec.ppt_to_date >= v_fin_yr_start))
      then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='Payment period must be earlier than current financial year when it is a "Backpay Payment".';
      end if;
      */
      -- Conrad Kwong @ 2011-08-12, Form# HRIS-09031, New Payment Type "D011P - Salary Deduction for Previous Financial Year
      --    for payment period earlier than current financial year, enforce users to choose A011P / D011P payment code
      if least(p_rec.ppt_fr_date, p_rec.ppt_to_date) < v_fin_yr_start and
         p_rec.ppt_paym_code NOT IN ('A011P', 'D011P', 'A042C')
      then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='For payment period earlier than current financial year, please use payment type "Salary Deduction for Previous Financial Year", "Backpay Payment for Previous Financial Year" or "Leave Encashment".';
      end if;
      if least(p_rec.ppt_fr_date, p_rec.ppt_to_date) >= v_fin_yr_start and
         p_rec.ppt_paym_code IN ('A011P', 'D011P')
      then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='For payment period in current financial year, payment type "Salary Deduction for Previous Financial Year" or "Backpay Payment for Previous Financial Year" not allowed.';
      end if;
      ----------------------------------------------------------------------------------------------------------------------
    end if;
    /* ------------------------------ Checking of Payment Code ------------------------------ */
    if p_rec.ppt_paym_code is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Payment Code] is a mandatory field';
    end if;
    /* ------------------------------ Checking of Pay Rate ------------------------------ */
    if p_rec.ppt_pay_rate is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Pay Rate] is a mandatory field';
    elsif p_rec.ppt_pay_rate not in ('M','D','H','O') then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Pay Rate] is either "Hourly", "Daily", "Monthly" or "Others".';
    /* ------------------------------ Checking of Pay Rate Amount ------------------------------ */
    elsif p_rec.ppt_pay_rate_amt is null and p_rec.ppt_pay_rate in ('M','D','H') then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Pay Rate Amount] is a mandatory field';
    elsif nvl(p_rec.ppt_pay_rate_amt,0) <> nvl(pkg_info.get_pay_rate_amt(p_rec.ppt_cntr_ctr, p_rec.ppt_cntr_yr, p_rec.ppt_cntr_sqn, p_rec.ppt_pay_rate),0)
          and p_rec.ppt_pay_rate in ('M','D','H') then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Pay Rate Amount] is not equal to contract pay rate amount ('||
                              pkg_format.money(pkg_info.get_pay_rate_amt(p_rec.ppt_cntr_ctr, p_rec.ppt_cntr_yr, p_rec.ppt_cntr_sqn, p_rec.ppt_pay_rate))||').';
    end if;
    /* ------------------------------ Checking of Numerator of Quantity Fraction ------------------------------ */
    if p_rec.ppt_qty_fract1 is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Numerator of Quantity] is a mandatory field';
    end if;
    /* ------------------------------ Checking of Denominator of Quantity Fraction ------------------------------ */
    if p_rec.ppt_qty_fract2 is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Denominator of Quantity] is a mandatory field';
    end if;
    /* ------------------------------ Checking of Numerator of Pay Fraction ------------------------------ */
    if p_rec.ppt_pay_fract1 is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Numerator of Pay Fraction] is a mandatory field';
    end if;
    /* ------------------------------ Checking of Denominator of Pay Fraction ------------------------------ */
    if p_rec.ppt_pay_fract2 is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Denominator of Pay Fraction] is a mandatory field';
    end if;
    /* ------------------------------ Checking of Autoday Count Flag ------------------------------ */
    if p_rec.ppt_autoday_count is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Autoday Count] is a mandatory field';
    elsif p_rec.ppt_autoday_count not in ('Y','N') then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Autoday Count] is either "Yes" or "No".';
    end if;
  -------------------------------------------------------------------------------------------------------
  -- Conrad Kwong @ 2017-04-10, Form# HRISPT-17002, Validate Chinese Character
  if p_rec.ppt_remarks is not null and is_chinese(p_str => p_rec.ppt_remarks) = 'Y' then
    v_msg.extend;
    v_msg(v_msg.count).msg_type:='E';
    v_msg(v_msg.count).msg:='Chinese characters is not allowed in [Remarks].';
  end if;
  -------------------------------------------------------------------------------------------------------

    /* ---------------------------------------------------------------
    -- Add by Raymond @ 20070525: A/C Allocation Modification
    -- Account Checking Logic:
    -- For IMC SS, Can only charge to itself
    -- For Other IMC, Can charge to itself and Project A/C
    -- For TW Centre, cannot charge to IMC Centre
    ------------------------------------------------------------------*/
    --Add by Raymond @ 20070727
    for i in p_rec.ppa_chrg_ctr.first .. p_rec.ppa_chrg_ctr.last loop
        if ctrlac_chk(p_rec.ppt_cntr_ctr,p_rec.ppa_chrg_ctr(i), p_rec.ppa_chrg_acc(i)) = False then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          v_msg(v_msg.count).msg:='You must input the charge centre. (Row '||i||')';
        end if;
        if ctrlac_chk2(p_rec.ppt_cntr_ctr,p_rec.ppa_chrg_ctr(i), p_rec.ppa_pfund_acc(i)) = False then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          v_msg(v_msg.count).msg:='You must not input the charge centre. (Row '||i||')';
        end if;
    end loop;
    -----------------------------
    if is_imc_centre(p_rec.ppt_cntr_ctr) then
       if is_ss(p_rec.ppt_cntr_ctr) then
          for i in p_rec.ppa_chrg_ctr.first .. p_rec.ppa_chrg_ctr.last loop
              if ss_ac_chk(p_rec.ppt_cntr_ctr, p_rec.ppa_chrg_ctr(i),p_rec.ppa_chrg_acc(i)) = false or ss_ac_chk(p_rec.ppt_cntr_ctr, p_rec.ppa_chrg_ctr(i), p_rec.ppa_pfund_acc(i)) = false then
                v_msg.extend;
                v_msg(v_msg.count).msg_type:='E';
                v_msg(v_msg.count).msg:='You can only charge to the same centre for IMC Secondary School Contract (Row '||i||')';
              end if;
           end loop;
       else
          for i in p_rec.ppa_chrg_ctr.first .. p_rec.ppa_chrg_ctr.last loop
              if imc_ac_chk(p_rec.ppt_cntr_ctr, p_rec.ppa_chrg_ctr(i), p_rec.ppa_chrg_acc(i)) = false or imc_ac_chk(p_rec.ppt_cntr_ctr, p_rec.ppa_chrg_ctr(i), p_rec.ppa_pfund_acc(i)) = false then
                v_msg.extend;
                v_msg(v_msg.count).msg_type:='E';
                v_msg(v_msg.count).msg:='You can only charge to the same centre or correct project A/C (Row '||i||')';
              end if;
           end loop;
       end if;
       for i in p_rec.ppa_chrg_ctr.first .. p_rec.ppa_chrg_ctr.last loop
         if imc_ctrlac_chk(p_rec.ppt_cntr_ctr, p_rec.ppa_chrg_ctr(i), p_rec.ppa_chrg_acc(i)) = false then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='Charge centre cannot be blank for this charge A/C (Row '||i||')';
         end if;
         if imc_ctrlac_chk(p_rec.ppt_cntr_ctr, p_rec.ppa_chrg_ctr(i), p_rec.ppa_pfund_acc(i)) = false then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='Charge centre cannot be blank for this PFund A/C (Row '||i||')';
         end if;
       end loop;
    else
      -----------------------------------------------------------------------------
      -- Conrad Kwong @ 2018-10-04, Form# HRIS-19007, change for KG/NS centre
      if is_kgns_centre_YN(p_rec.ppt_cntr_ctr) = 'Y' then
        for i in p_rec.ppa_chrg_ctr.first .. p_rec.ppa_chrg_ctr.last loop
          if kgns_ac_chk(p_rec.ppt_cntr_ctr, p_rec.ppa_chrg_ctr(i), p_rec.ppa_chrg_acc(i)) = false or kgns_ac_chk(p_rec.ppt_cntr_ctr, p_rec.ppa_chrg_ctr(i), p_rec.ppa_pfund_acc(i)) = false then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='You can only charge to TW A/C or correct project A/C (Row '||i||')';
          end if;
          if kgns_ctrlac_chk(p_rec.ppt_cntr_ctr, p_rec.ppa_chrg_ctr(i), p_rec.ppa_chrg_acc(i)) = false then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='Charge centre cannot be blank for this charge A/C (Row '||i||')';
          end if;
          if kgns_ctrlac_chk(p_rec.ppt_cntr_ctr, p_rec.ppa_chrg_ctr(i), p_rec.ppa_pfund_acc(i)) = false then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='Charge centre cannot be blank for this PFund A/C (Row '||i||')';
          end if;
        end loop;
      else
      -----------------------------------------------------------------------------
        for i in p_rec.ppa_chrg_ctr.first .. p_rec.ppa_chrg_ctr.last loop
           if tw_ac_chk(p_rec.ppt_cntr_ctr, p_rec.ppa_chrg_ctr(i), p_rec.ppa_chrg_acc(i)) = false or tw_ac_chk(p_rec.ppt_cntr_ctr, p_rec.ppa_chrg_ctr(i), p_rec.ppa_pfund_acc(i)) = false then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              -----------------------------------------------------------------------------
              -- Conrad Kwong @ 2018-10-04, Form# HRIS-19007, change for KG/NS centre
              --v_msg(v_msg.count).msg:='You can only charge to TW A/C or correct project A/C (Row '||i||')';
              v_msg(v_msg.count).msg:='You can only charge to TW A/C, KG/NS A/C or correct project A/C (Row '||i||')';
              -----------------------------------------------------------------------------
           end if;
           if tw_ctrlac_chk(p_rec.ppt_cntr_ctr, p_rec.ppa_chrg_ctr(i), p_rec.ppa_chrg_acc(i)) = false then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='Charge centre cannot be blank for this charge A/C (Row '||i||')';
           end if;
           if tw_ctrlac_chk(p_rec.ppt_cntr_ctr, p_rec.ppa_chrg_ctr(i), p_rec.ppa_pfund_acc(i)) = false then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='Charge centre cannot be blank for this PFund A/C (Row '||i||')';
           end if;
        end loop;
      -----------------------------------------------------------------------------
      -- Conrad Kwong @ 2018-10-04, Form# HRIS-19007, change for KG/NS centre
      end if;
      -----------------------------------------------------------------------------
    end if;
    for i in p_rec.ppa_chrg_ctr.first .. p_rec.ppa_chrg_ctr.last loop
       if chk_proj_ac(p_rec.ppa_chrg_acc(i), p_rec.ppa_chrg_ctr(i)) = false then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          v_msg(v_msg.count).msg:='No need to input charge centre for this Charge A/C (Row '||i||')';
       end if;
    end loop;

    -- End of Addition ----------------------------------------------------

    /* ------------------------------ Checking of pay rate, period when autoday_count flag is true ------------------------------ */
    if p_rec.ppt_autoday_count = 'Y' then
      if p_rec.ppt_pay_rate = 'M' then
      /* cross verify following criteria when pay rate is "monthly"
       * -- the payment period should be same month
       * -- the numerator and denominator value of quantity */
        if to_char(p_rec.ppt_fr_date, 'MMYYYY') <> to_char(p_rec.ppt_to_date, 'MMYYYY') then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          v_msg(v_msg.count).msg:='Invalid of [Payment Period]. Both period [From Date] and [To Date] should be in same month.';
        else
          if (p_rec.ppt_to_date-p_rec.ppt_fr_date+1) <> p_rec.ppt_qty_fract1 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='Invalid value of [Numerator of Quantity].';
          end if;
          if to_number(to_char(last_day(p_rec.ppt_to_date),'DD')) <> p_rec.ppt_qty_fract2 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='Invalid value of [Denominator of Quantity].';
          end if;
        end if;
      elsif p_rec.ppt_pay_rate = 'D' then
      /* cross verify following criteria when pay rate is "daily"
       * -- the numerator and denominator value of quantity */
        if (p_rec.ppt_to_date-p_rec.ppt_fr_date+1) <> p_rec.ppt_qty_fract1 then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          v_msg(v_msg.count).msg:='Invalid value of [Numerator of Quantity].';
        end if;
        if p_rec.ppt_qty_fract2 <> 1 then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          v_msg(v_msg.count).msg:='Invalid value of [Denominator of Quantity].';
        end if;
      elsif p_rec.ppt_pay_rate = 'H' then
      /* No autoday count allow then pay rate is "hourly" */
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='Invalid value of [Pay Rate]. Pay rate cannot be "Hourly" when autoday count flag is checked.';
      end if;
    end if;

    /* ------------------------------ Checking of Gorss Amount ------------------------------ */
    if p_rec.ppt_gross_amt is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Gross Amount] is a mandatory field';
    elsif p_rec.ppt_pay_rate in ('H','D','M') then
      v_gross_amt := NVL(p_rec.ppt_pay_rate_amt, 0) *
                     NVL(p_rec.ppt_qty_fract1 / p_rec.ppt_qty_fract2, 0) * NVL(p_rec.ppt_pay_fract1 / p_rec.ppt_pay_fract2, 0);
      if p_rec.ppt_gross_amt <> round(v_gross_amt, 2) then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='Invalid value of [Gross Amount]. Please verify [Pay Rate], [Quantity] and [Pay Fraction].';
      end if;
    end if;

    /* ------------------------------ Checking of Charge Account Record ------------------------------ */
    if p_rec.ppa_chrg_ctr.exists(1) then
      v_chrg_total := 0;   -- initial the total charge amount
      for i in p_rec.ppa_chrg_ctr.first .. p_rec.ppa_chrg_ctr.last loop
        /* Checking of Charge Centre */
        if p_rec.ppa_chrg_ctr(i) is not null then
          v_valid_code :=valid_ctr_code(p_rec.ppa_chrg_ctr(i),p_rec.ppt_fr_date,p_rec.ppt_to_date,'C');
          if v_valid_code!=1 then
            if v_valid_code = -1 then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='Charge Centre does not exist. (row '|| i ||')';
            elsif v_valid_code = -2 then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='Charge Centre is not effective yet. (row '|| i ||')';
            elsif v_valid_code = -3 then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='W';
              v_msg(v_msg.count).msg:='From date is later than charge centre dormant date. (row '|| i ||')';
            elsif v_valid_code = -4 then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='From date is later than charge centre deletion date. (row '|| i ||')';
            elsif v_valid_code = -5 then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='W';
              v_msg(v_msg.count).msg:='To date is later than charge centre dormant date. (row '|| i ||')';
            elsif v_valid_code = -6 then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='To date is later than charge centre deletion date. (row '|| i ||')';
            elsif v_valid_code = -7 then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='Charge Centre should not be used for charging purpose. (row '|| i ||')';
            else
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='Invaild centre of [Charge Centre]. (row '|| i ||')';
            end if;
          end if;
          --Add by Raymond @ 20070517: A/C Allocation Modification
          --Check whether the serv ctr and chrg ctr are both imc or not imc
          /*if pkg_validate.valid_chrg_ac(p_rec.ppt_cntr_ctr,p_rec.ppa_chrg_ctr(i))=0 then
             v_msg.extend;
             v_msg(v_msg.count).msg_type:='E';
             v_msg(v_msg.count).msg:='The serving centre and the salary charge centre should be both IMC or not IMC.(Row '||i||')';
          end if;*/
          -----------------------------------------------------------------
        end if;
        /* Checking of Charge Account */
        if p_rec.ppa_chrg_acc(i) is null then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          v_msg(v_msg.count).msg:='[Charge Account] is a mandatory field. (row '|| i ||')';
        elsif pkg_validate.valid_acc_code(p_rec.ppa_chrg_acc(i),p_rec.ppt_fr_date,p_rec.ppt_to_date)!=1 then
          v_valid_code :=valid_acc_code(p_rec.ppa_chrg_acc(i),p_rec.ppt_fr_date,p_rec.ppt_to_date);
          if v_valid_code = -1 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='[Charge Account] does not exist. (row '|| i ||')';
          elsif v_valid_code = -2 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='[Charge Account] is not effective yet. (row '|| i ||')';
          elsif v_valid_code = -3 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='W';
            v_msg(v_msg.count).msg:='From date is later than charge account dormant date. (row '|| i ||')';
          elsif v_valid_code = -4 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='From date is later than charge account deletion date. (row '|| i ||')';
          elsif v_valid_code = -5 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='W';
            v_msg(v_msg.count).msg:='To date is later than charge account dormant date. (row '|| i ||')';
          elsif v_valid_code = -6 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='To date is later than charge account deletion date. (row '|| i ||')';
          else
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='Invalid account of [Charge Account]. (row '|| i ||')';
          end if;
        --Add by Raymond @ 20070517: A/C Allocation Modification
        --Check whether the users input correct type of A/C as the charging A/C
        /*elsif pkg_validate.valid_ac_typ(p_rec.ppa_chrg_ctr(i),p_rec.ppa_chrg_acc(i),1)!=1 then
           v_msg.extend;
           v_msg(v_msg.count).msg_type:='W';
           v_msg(v_msg.count).msg:='Charging A/C cannot be found.(Row '||i||')';*/
        -----------------------------------------------------------------
        end if;
        /* Checking of PFund Account */
        if p_rec.ppa_pfund_acc(i) is null then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          v_msg(v_msg.count).msg:='[PFund Account] is a mandatory field. (row '|| i ||')';
        elsif pkg_validate.valid_acc_code(p_rec.ppa_pfund_acc(i),p_rec.ppt_fr_date,p_rec.ppt_to_date)!=1 then
          v_valid_code :=valid_acc_code(p_rec.ppa_pfund_acc(i),p_rec.ppt_fr_date,p_rec.ppt_to_date);
          if v_valid_code <> 1 then
            if v_valid_code = -1 then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='[Charge Account] does not exist. (row '|| i ||')';
            elsif v_valid_code = -2 then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='[Charge Account] is not effective yet. (row '|| i ||')';
            elsif v_valid_code = -3 then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='W';
              v_msg(v_msg.count).msg:='From date is later than charge account dormant date. (row '|| i ||')';
            elsif v_valid_code = -4 then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='From date is later than charge account deletion date. (row '|| i ||')';
            elsif v_valid_code = -5 then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='W';
              v_msg(v_msg.count).msg:='To date is later than charge account dormant date. (row '|| i ||')';
            elsif v_valid_code = -6 then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='To date is later than charge account deletion date. (row '|| i ||')';
            else
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='Invalid account of [Charge Account]. (row '|| i ||')';
            end if;
          end if;
        --Add by Raymond @ 20070517: A/C Allocation Modification
        --Check whether the users input correct type of A/C as the charging A/C
        /*elsif pkg_validate.valid_ac_typ(p_rec.ppa_chrg_ctr(i),p_rec.ppa_pfund_acc(i),2)!=1 then
           v_msg.extend;
           v_msg(v_msg.count).msg_type:='W';
           v_msg(v_msg.count).msg:='PF A/C cannot be found.(Row '||i||')';*/
        -----------------------------------------------------------------
        end if;
        /* Checking of Charge Amount */
        if p_rec.ppa_chrg_amt(i) is null then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          v_msg(v_msg.count).msg:='[Charge Amount] is a mandatory field. (row '|| i ||')';
        end if;
        /* Matching between centre and control account */
        if not valid_ctrtyp_ctrl_ac(p_rec.ppa_chrg_ctr(i),p_rec.ppa_chrg_acc(i)) then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          v_msg(v_msg.count).msg:='Invalid control account for [Charge Centre] and [Charge Account]. Please set [Charge Centre] to blank if [Charge Account] or [PFund Charge Account] is a project account. Please contact Finance for details. (row '|| i ||')';
        end if;
        if not valid_ctrtyp_ctrl_ac(p_rec.ppa_chrg_ctr(i),p_rec.ppa_pfund_acc(i)) then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          v_msg(v_msg.count).msg:='Invalid control account for [Charge Centre] and [PFund Charge Account]. Please set [Charge Centre] to blank if [Charge Account] or [PFund Charge Account] is a project account. Please contact Finance for details. (row '|| i ||')';
        end if;
        v_chrg_total := v_chrg_total + NVL(p_rec.ppa_chrg_amt(i),0);
      end loop;
      /* ------------------------------ Checking of Gorss Amount and Charge Account Amount ------------------------------ */
      if p_rec.ppt_gross_amt <> v_chrg_total then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='Invalid value of [Gross Amount '||p_rec.ppt_gross_amt||'] or [Charge Account Amount '||v_chrg_total||']. Two of them should be equal.';
      end if;
    else
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='Invalid of [Charge Account]. At least input one charge account record.';
    end if;

    return v_msg;

  exception
    when no_data_found then
         v_msg.extend;
         v_msg(v_msg.count).msg_type:='E';
         v_msg(v_msg.count).msg:='No Contract Found!';
         return v_msg;
    when others then
         v_msg.extend;
         v_msg(v_msg.count).msg_type:='E';
         v_msg(v_msg.count).msg:= SQLERRM||'! [pkg_validation.validation (line no: 2335]';
         return v_msg;
  end validation;

  function check_stfno(p_stfno in hr_staff.stf_no%type) return boolean is
    v_count pls_integer:= 0;
  begin
    select count(1) into v_count
    from hr_staff
    where stf_no=p_stfno;

    if v_count = 1 then
      return true;
    end if;
    return false;
  end check_stfno;


  function check_cntrno(p_cntr_ctr in hr_ptcntr.pct_cntr_ctr%type,
                        p_cntr_yr in hr_ptcntr.pct_cntr_yr%type,
                        p_cntr_sqn in hr_ptcntr.pct_cntr_sqn%type)
  return boolean
  is  v_count pls_integer := 0;
  begin
    v_count := 0;
    select count(1) into v_count
    from hr_ptcntr
    where pct_cntr_ctr = p_cntr_ctr
    and pct_cntr_yr = p_cntr_yr
    and pct_cntr_sqn = p_cntr_sqn;

    if v_count = 1 then
      return true;
    elsif v_count = 0 then
     /* -- search in hr_ptcntr_tx table
      select count(1) into v_count
      from hr_ptcntr_tx
      where ptt_cntr_ctr = p_cntr_ctr
      and ptt_cntr_yr = p_cntr_yr
      and ptt_cntr_sqn = p_cntr_sqn;
      -- and some status, e.g. mark delete, approve.... cutoff...etc.
     */
      if v_count = 1 then
        return true;
      else
        return false;
      end if;
    end if;
  end check_cntrno;

  function is_account(p_ac_code in varchar2)
  return boolean
  as
  /* Program ID: is_account
   * Desc: count number of account by input account code
   *       from hr_ac_name, return true when count is 1 only
   * Create: 02/02/2004
   */
    v_count pls_integer := 0;
  begin
    select count(1) into v_count
    from hr_ac_name
    where trim(acn_ac_code) = trim(p_ac_code);
    if v_count = 1 then
      return true;
    else
      return false;
    end if;
  end is_account;



  function is_centre(p_ctr_code in varchar2)
  return boolean
  as
  /* Program ID: is_centre
   * Desc: count number of centre by input value
   *       from hr_centre, return true when count is 1 only
   * Create: 02/02/2004
   */
    v_count pls_integer := 0;
  begin
    select count(1) into v_count
    from hr_centre
    where trim(ctr_code) = trim(p_ctr_code);
    if v_count = 1 then
      return true;
    else
      return false;
    end if;
  end is_centre;

  function validation(p_mode varchar2:='insert', p_rec pkg_rec.rec_pt_pfadj_tx) return pkg_rec.lst_rec_msg as
  /******************************************************
   * Validation of PFund adjustment record.
   * Use in pt_pfadj_form, pt_pfadj_dtls, pkgf_pt_pfadj
   ******************************************************/
    v_msg          pkg_rec.lst_rec_msg;
    v_count        pls_integer := 0;
    v_cntr_start   hr_ptcntr.pct_cntr_end%type;          -- start date of the contract
    v_cntr_end     hr_ptcntr.pct_cntr_end%type;          -- end date of the contract
    v_permit_xdate hr_staff.stf_permit_xdate%type;       -- staff's permit expiry date
    v_pyrl_mth     date := pkg_info.get_pyrl_mth;
    v_valid_code number;  -- validation code for valid_ctr_code and valid_acc_code
  begin
    v_msg:=pkg_rec.lst_rec_msg();

    ------------ If it is marked as deleted, do not valid -----------
    if p_rec.pfa_mrk_del='Y' then
      return v_msg;
    end if;
    -----------------------------------------------------------------

    /* ---------------------------- Checking of Referecne No. ---------------------------- */
    if p_rec.pfa_refno is null and p_mode not like '%insert%' then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Reference No.] is a mandatory field.';
    elsif p_mode like '%insert%' then
      select sum(counts) into v_count
      from (
        select count(1) counts
        from hr_pt_pfadj_tx
        where pfa_refno = p_rec.pfa_refno
      );
      if v_count >=1 then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='The [Reference No.] ('|| p_rec.pfa_refno ||') is already used in our system';
      end if;
    end if;
    /* ------------------------------ Checking of From Date ------------------------------ */
    if p_rec.pfa_fr_date is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Period From Date] is a mandatory field';
    /* ------------ Checking Watchman Permit Expiry Date against Payment To Date ------------ */
    elsif pkg_info.is_watchman_permit_xdate(p_rec.pfa_cntr_ctr, p_rec.pfa_cntr_yr, p_rec.pfa_cntr_sqn, v_permit_xdate) then
      if p_rec.pfa_fr_date >= v_permit_xdate or v_permit_xdate is null then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='W';
        v_msg(v_msg.count).msg:='Warning: [Period From Date] is out of staff''s permit expiry date ('||v_permit_xdate||')';
      end if;
    end if;
    /* ------------------------------ Checking of To Date ------------------------------ */
    if p_rec.pfa_to_date is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Period To Date] is a mandatory field';
    end if;
    /* ------------------------------ Checking of From Date/To Date period ------------------------------ */
    if p_rec.pfa_fr_date is not null and p_rec.pfa_to_date is not null then
      if p_rec.pfa_fr_date>p_rec.pfa_to_date then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='Payment period [To Date] must be on or after [From Date]';
      end if;

      -- get contract start date and end date
      select pct_cntr_start, pct_cntr_end
      into v_cntr_start, v_cntr_end
      from hr_ptcntr
      where pct_cntr_ctr = p_rec.pfa_cntr_ctr
      and pct_cntr_yr = p_rec.pfa_cntr_yr
      and pct_cntr_sqn = p_rec.pfa_cntr_sqn;
      -- payment period must be within contract serve period
      if p_rec.pfa_fr_date < v_cntr_start then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='Payment period [From Date] is out of contract period (Contract Period: '||v_cntr_start||' to '||v_cntr_end||')';
      end if;
      if p_rec.pfa_to_date > v_cntr_end then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='Payment period [To Date] is out of contract period (Contract Period: '||v_cntr_start||' to '||v_cntr_end||')';
      end if;

      if not
         ((pkg_pt_pyrlproc_ctrl.get_batch_no = 0 and p_rec.pfa_to_date <= last_day(v_pyrl_mth))
          or (pkg_pt_pyrlproc_ctrl.get_batch_no > 0 and p_rec.pfa_to_date <= last_day(add_months(v_pyrl_mth,1)))
          or (p_rec.pfa_to_date <= last_day(sysdate)))
      then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='Payment period cannot be post dated.';
      end if;
    end if;
    /* ------------------------------ Checking of Payment Code ------------------------------ */
    if p_rec.pfa_paym_code is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Payment Code] is a mandatory field';
    end if;

    /* ------------------------------ Checking of Charge Centre Record ------------------------------ */
    /* Checking of Gross Pay Charge Centre */
    if p_rec.pfa_sal_chrg_ctr is not null then
        v_valid_code := valid_ctr_code(p_rec.pfa_sal_chrg_ctr,p_rec.pfa_fr_date,p_rec.pfa_to_date ,'C');
        if v_valid_code !=1 then
            if v_valid_code = -1 then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='Gross Pay Charge Centre does not exist. ';
            elsif v_valid_code = -2 then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='Gross Pay Charge Centre is not effective yet.';
            elsif v_valid_code = -3 then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='W';
              v_msg(v_msg.count).msg:='From date is later than Gross Pay charge centre dormant date.';
            elsif v_valid_code = -4 then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='From date is later than Gross Pay charge centre deletion date.';
            elsif v_valid_code = -5 then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='W';
              v_msg(v_msg.count).msg:='To date is later than Gross Pay charge centre dormant date.';
            elsif v_valid_code = -6 then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='To date is later than Gross Pay charge centre deletion date.';
            elsif v_valid_code = -7 then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='Gross Pay Charge Centre should not be used for charging purpose.';
            else
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='Invaild centre of [Gross Pay Charge Centre].';
            end if;
        end if;
     end if;
    /* ---------------------------------------------------------------
      -- Add by Raymond @ 20070525: A/C Allocation Modification
      -- Account Checking Logic:
      -- For IMC SS, Can only charge to itself
      -- For Other IMC, Can charge to itself and Project A/C
      -- For TW Centre, cannot charge to IMC Centre
      ------------------------------------------------------------------*/
      --Add by Raymond @ 20070727
      if ctrlac_chk(p_rec.pfa_cntr_ctr,p_rec.pfa_sal_chrg_ctr, p_rec.pfa_sal_chrg_acc) = False then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='You must input the charge centre. (Salary Charge)';
      end if;
      if ctrlac_chk2(p_rec.pfa_cntr_ctr,p_rec.pfa_sal_chrg_ctr, p_rec.pfa_sal_chrg_acc) = False then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='You must not input the charge centre. (Salary Charge)';
      end if;
      -----------------------------
      if is_imc_centre(p_rec.pfa_cntr_ctr) then
         if is_ss(p_rec.pfa_cntr_ctr) then
            if ss_ac_chk(p_rec.pfa_cntr_ctr, p_rec.pfa_sal_chrg_ctr, p_rec.pfa_sal_chrg_acc) = false then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='You can only charge to the same centre as the serving centre only (Salary Payment)';
             end if;
          else
              if imc_ac_chk(p_rec.pfa_cntr_ctr, p_rec.pfa_sal_chrg_ctr,p_rec.pfa_sal_chrg_acc) = false then
                v_msg.extend;
                v_msg(v_msg.count).msg_type:='E';
                v_msg(v_msg.count).msg:='You can only charge to the same centre as the serving centre or project A/C (Salary Payment)';
              end if;
          end if;
          if imc_ctrlac_chk(p_rec.pfa_cntr_ctr, p_rec.pfa_sal_chrg_ctr, p_rec.pfa_sal_chrg_acc) = false then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='Charge centre cannot be blanked for this Salary Payment A/C';
          end if;
      else
        -----------------------------------------------------------------------------
        -- Conrad Kwong @ 2018-10-04, Form# HRIS-19007, change for KG/NS centre
        if is_kgns_centre_YN(p_rec.pfa_cntr_ctr) = 'Y' then
          if kgns_ac_chk(p_rec.pfa_cntr_ctr, p_rec.pfa_sal_chrg_ctr, p_rec.pfa_sal_chrg_acc) = false then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='You can only charge to TW A/C or correct project A/C (Salary Payment)';
          end if;
          if kgns_ctrlac_chk(p_rec.pfa_cntr_ctr, p_rec.pfa_sal_chrg_ctr, p_rec.pfa_sal_chrg_acc) = false then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='Charge centre cannot be blank for Salary Payment A/C';
          end if;
        else
        -----------------------------------------------------------------------------
          if tw_ac_chk(p_rec.pfa_cntr_ctr, p_rec.pfa_sal_chrg_ctr,p_rec.pfa_sal_chrg_acc) = false then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            -----------------------------------------------------------------------------
            -- Conrad Kwong @ 2018-10-04, Form# HRIS-19007, change for KG/NS centre
            --v_msg(v_msg.count).msg:='You can only charge to the TW centre or project A/C (Salary Payment)';
            v_msg(v_msg.count).msg:='You can only charge to TW A/C, KG/NS A/C or correct project A/C (Salary Payment)';
            -----------------------------------------------------------------------------
          end if;
          if tw_ctrlac_chk(p_rec.pfa_cntr_ctr, p_rec.pfa_sal_chrg_ctr, p_rec.pfa_sal_chrg_acc) = false then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='Charge centre cannot be blanked for Salary Payment A/C';
          end if;
        -----------------------------------------------------------------------------
        -- Conrad Kwong @ 2018-10-04, Form# HRIS-19007, change for KG/NS centre
        end if;
        -----------------------------------------------------------------------------
      end if;
      if chk_proj_ac(p_rec.pfa_sal_chrg_acc, p_rec.pfa_sal_chrg_ctr) = false then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='No need to input charge centre for this Salary Payment A/C';
      end if;
      -- End of Addition ----------------------------------------------------

    /* Checking of ERMC Charge Centre */
    if p_rec.pfa_ermc_chrg_ctr is not null then
      v_valid_code := valid_ctr_code(p_rec.pfa_ermc_chrg_ctr,p_rec.pfa_fr_date,p_rec.pfa_to_date ,'C');
      if v_valid_code!=1 then
          if v_valid_code = -1 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='ERC Charge Centre does not exist. ';
          elsif v_valid_code = -2 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='ERC Charge Centre is not effective yet.';
          elsif v_valid_code = -3 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='W';
            v_msg(v_msg.count).msg:='From date is later than ERC charge centre dormant date.';
          elsif v_valid_code = -4 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='From date is later than ERC charge centre deletion date.';
          elsif v_valid_code = -5 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='W';
            v_msg(v_msg.count).msg:='To date is later than ERC charge centre dormant date.';
          elsif v_valid_code = -6 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='To date is later than ERC charge centre deletion date.';
          elsif v_valid_code = -7 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='ERC Charge Centre should not be used for charging purpose.';
          else
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='Invaild centre of [ERC Charge Centre].';
          end if;
        end if;
      end if;
     /* ---------------------------------------------------------------
      -- Add by Raymond @ 20070525: A/C Allocation Modification
      -- Account Checking Logic:
      -- For IMC SS, Can only charge to itself
      -- For Other IMC, Can charge to itself and Project A/C
      -- For TW Centre, cannot charge to IMC Centre
      ------------------------------------------------------------------*/
      --Add by Raymond @ 20070727
      if ctrlac_chk(p_rec.pfa_cntr_ctr,p_rec.pfa_ermc_chrg_ctr, p_rec.pfa_ermc_chrg_acc) = False then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='You must input the charge centre. (ERMC)';
      end if;
      if ctrlac_chk2(p_rec.pfa_cntr_ctr,p_rec.pfa_ermc_chrg_ctr, p_rec.pfa_ermc_chrg_acc) = False then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='You must not input the charge centre. (ERMC)';
      end if;
      -----------------------------
      if is_imc_centre(p_rec.pfa_cntr_ctr) then
         if is_ss(p_rec.pfa_cntr_ctr) then
            if ss_ac_chk(p_rec.pfa_cntr_ctr, p_rec.pfa_ermc_chrg_ctr,p_rec.pfa_ermc_chrg_acc) = false then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='You can only charge to the same centre as the serving centre only (ERC Payment)';
             end if;
          else
              if imc_ac_chk(p_rec.pfa_cntr_ctr, p_rec.pfa_ermc_chrg_ctr,p_rec.pfa_ermc_chrg_acc) = false then
                v_msg.extend;
                v_msg(v_msg.count).msg_type:='E';
                v_msg(v_msg.count).msg:='You can only charge to the same centre as the serving centre or project A/C (ERC Payment)';
              end if;
          end if;
          if imc_ctrlac_chk(p_rec.pfa_cntr_ctr, p_rec.pfa_ermc_chrg_ctr, p_rec.pfa_ermc_chrg_acc) = false then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='Charge centre cannot be blanked for this ERC A/C';
          end if;
      else
        -----------------------------------------------------------------------------
        -- Conrad Kwong @ 2018-10-04, Form# HRIS-19007, change for KG/NS centre
        if is_kgns_centre_YN(p_rec.pfa_cntr_ctr) = 'Y' then
          if kgns_ac_chk(p_rec.pfa_cntr_ctr, p_rec.pfa_ermc_chrg_ctr, p_rec.pfa_ermc_chrg_acc) = false then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='You can only charge to TW A/C or correct project A/C (ERC Payment)';
          end if;
          if kgns_ctrlac_chk(p_rec.pfa_cntr_ctr, p_rec.pfa_ermc_chrg_ctr, p_rec.pfa_ermc_chrg_acc) = false then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='Charge centre cannot be blank for this ERC A/C';
          end if;
        else
        -----------------------------------------------------------------------------
          if tw_ac_chk(p_rec.pfa_cntr_ctr, p_rec.pfa_ermc_chrg_ctr,p_rec.pfa_ermc_chrg_acc) = false then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            -----------------------------------------------------------------------------
            -- Conrad Kwong @ 2018-10-04, Form# HRIS-19007, change for KG/NS centre
            --v_msg(v_msg.count).msg:='You can only charge to the TW centre or project A/C (ERC Payment)';
            v_msg(v_msg.count).msg:='You can only charge to TW A/C, KG/NS A/C or correct project A/C (ERC Payment)';
            -----------------------------------------------------------------------------
          end if;
          if tw_ctrlac_chk(p_rec.pfa_cntr_ctr, p_rec.pfa_ermc_chrg_ctr, p_rec.pfa_ermc_chrg_acc) = false then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='Charge centre cannot be blanked for this ERC A/C';
          end if;
        -----------------------------------------------------------------------------
        -- Conrad Kwong @ 2018-10-04, Form# HRIS-19007, change for KG/NS centre
        end if;
        -----------------------------------------------------------------------------
      end if;
      if chk_proj_ac(p_rec.pfa_ermc_chrg_acc, p_rec.pfa_ermc_chrg_ctr) = false then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='No need to input charge centre for this ERC A/C';
      end if;
      -- End of Addition ----------------------------------------------------

     /* Checking of ERAVC Charge Centre */
     if p_rec.pfa_eravc_chrg_ctr is not null then
      v_valid_code := valid_ctr_code(p_rec.pfa_eravc_chrg_ctr,p_rec.pfa_fr_date,p_rec.pfa_to_date ,'C');
      if v_valid_code!=1 then
        if v_valid_code = -1 then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          v_msg(v_msg.count).msg:='ERAVC Charge Centre does not exist. ';
        elsif v_valid_code = -2 then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          v_msg(v_msg.count).msg:='ERAVC Charge Centre is not effective yet.';
        elsif v_valid_code = -3 then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='W';
          v_msg(v_msg.count).msg:='From date is later than ERAVC charge centre dormant date.';
        elsif v_valid_code = -4 then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          v_msg(v_msg.count).msg:='From date is later than ERAVC charge centre deletion date.';
        elsif v_valid_code = -5 then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='W';
          v_msg(v_msg.count).msg:='To date is later than ERAVC charge centre dormant date.';
        elsif v_valid_code = -6 then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          v_msg(v_msg.count).msg:='To date is later than ERAVC charge centre deletion date.';
        elsif v_valid_code = -7 then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          v_msg(v_msg.count).msg:='ERAVC Charge Centre should not be used for charging purpose.';
        else
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          v_msg(v_msg.count).msg:='Invaild centre of [ERAVC Charge Centre].';
        end if;
     end if;
    end if;
     /* ---------------------------------------------------------------
      -- Add by Raymond @ 20070525: A/C Allocation Modification
      -- Account Checking Logic:
      -- For IMC SS, Can only charge to itself
      -- For Other IMC, Can charge to itself and Project A/C
      -- For TW Centre, cannot charge to IMC Centre
      ------------------------------------------------------------------*/
      --Add by Raymond @ 20070727
      if ctrlac_chk(p_rec.pfa_cntr_ctr,p_rec.pfa_eravc_chrg_ctr, p_rec.pfa_eravc_chrg_acc) = False then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='You must input the charge centre. (ERAVC)';
      end if;
      if ctrlac_chk2(p_rec.pfa_cntr_ctr,p_rec.pfa_eravc_chrg_ctr, p_rec.pfa_eravc_chrg_acc) = False then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='You must not input the charge centre. (ERAVC)';
      end if;
      -----------------------------
      if is_imc_centre(p_rec.pfa_cntr_ctr) then
         if is_ss(p_rec.pfa_cntr_ctr) then
            if ss_ac_chk(p_rec.pfa_cntr_ctr, p_rec.pfa_eravc_chrg_ctr, p_rec.pfa_eravc_chrg_acc) = false then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='You can only charge to the same centre as the serving centre only (ERAVC Payment)';
             end if;
          else
              if imc_ac_chk(p_rec.pfa_cntr_ctr, p_rec.pfa_eravc_chrg_ctr,p_rec.pfa_eravc_chrg_acc) = false then
                v_msg.extend;
                v_msg(v_msg.count).msg_type:='E';
                v_msg(v_msg.count).msg:='You can only charge to the same centre as the serving centre or project A/C (ERAVC Payment)';
              end if;
          end if;
          if imc_ctrlac_chk(p_rec.pfa_cntr_ctr, p_rec.pfa_eravc_chrg_ctr, p_rec.pfa_eravc_chrg_acc) = false then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='Charge centre cannot be blanked for this ERAVC A/C';
          end if;
      else
        -----------------------------------------------------------------------------
        -- Conrad Kwong @ 2018-10-04, Form# HRIS-19007, change for KG/NS centre
        if is_kgns_centre_YN(p_rec.pfa_cntr_ctr) = 'Y' then
          if kgns_ac_chk(p_rec.pfa_cntr_ctr, p_rec.pfa_eravc_chrg_ctr, p_rec.pfa_eravc_chrg_acc) = false then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='You can only charge to TW A/C or correct project A/C (ERAVC Payment)';
          end if;
          if kgns_ctrlac_chk(p_rec.pfa_cntr_ctr, p_rec.pfa_eravc_chrg_ctr, p_rec.pfa_eravc_chrg_acc) = false then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='Charge centre cannot be blank for this ERAVC A/C';
          end if;
        else
        -----------------------------------------------------------------------------
          if tw_ac_chk(p_rec.pfa_cntr_ctr, p_rec.pfa_eravc_chrg_ctr,p_rec.pfa_eravc_chrg_acc) = false then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            -----------------------------------------------------------------------------
            -- Conrad Kwong @ 2018-10-04, Form# HRIS-19007, change for KG/NS centre
            --v_msg(v_msg.count).msg:='You can only charge to the TW centre or project A/C (ERAVC Payment)';
            v_msg(v_msg.count).msg:='You can only charge to TW A/C, KG/NS A/C or correct project A/C (ERAVC Payment)';
            -----------------------------------------------------------------------------
          end if;
          if tw_ctrlac_chk(p_rec.pfa_cntr_ctr, p_rec.pfa_eravc_chrg_ctr, p_rec.pfa_eravc_chrg_acc) = false then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='Charge centre cannot be blanked for this ERAVC A/C';
          end if;
        -----------------------------------------------------------------------------
        -- Conrad Kwong @ 2018-10-04, Form# HRIS-19007, change for KG/NS centre
        end if;
        -----------------------------------------------------------------------------
      end if;
      if chk_proj_ac(p_rec.pfa_eravc_chrg_acc, p_rec.pfa_eravc_chrg_ctr) = false then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='No need to input charge centre for this ERAVC A/C';
      end if;
      -- End of Addition ----------------------------------------------------

    /* ------------------------------------------------------------------------------------------------*/

    /* ------------------------------ Checking of Charge Account Record ------------------------------ */
    /* Checking of Charge Account */

    if p_rec.pfa_sal_chrg_acc is null and p_rec.pfa_ermc_chrg_acc is null and p_rec.pfa_eravc_chrg_acc is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='Invalid of [Charge Account]. At least input one charge account record.';
    elsif (p_rec.pfa_sal_chrg_acc is not null and not is_account(p_rec.pfa_sal_chrg_acc)) or
          (p_rec.pfa_ermc_chrg_acc is not null and not is_account(p_rec.pfa_ermc_chrg_acc)) or
          (p_rec.pfa_eravc_chrg_acc is not null and not is_account(p_rec.pfa_eravc_chrg_acc))
    then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='Inputed value of [Charge Account] is invalidate';
    end if;
    /* Checking of Charge Amount */
    if p_rec.pfa_gross_amt is null and
       p_rec.pfa_eemc_amt is null and p_rec.pfa_eeavc_amt is null and
       p_rec.pfa_ermc_amt is null and p_rec.pfa_eravc_amt is null
    then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='Invalid of [Charge Amount]. At least input one charge amount record.';
    end if;

    /* Checking of Gross Pay Charge Account */
    if p_rec.pfa_sal_chrg_acc is not null then
      v_valid_code := valid_acc_code(p_rec.pfa_sal_chrg_acc,p_rec.pfa_fr_date,p_rec.pfa_to_date );
      if v_valid_code <> 1 then
          if v_valid_code = -1 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='[Gross Pay Charge Account] does not exist.';
          elsif v_valid_code = -2 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='[Gross Pay Charge Account] is not effective yet.';
          elsif v_valid_code = -3 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='W';
            v_msg(v_msg.count).msg:='From date is later than gross pay charge account dormant date.';
          elsif v_valid_code = -4 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='From date is later than gross pay charge account deletion date.';
          elsif v_valid_code = -5 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='W';
            v_msg(v_msg.count).msg:='To date is later than gross pay charge account dormant date.';
          elsif v_valid_code = -6 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='To date is later than gross pay charge account deletion date.';
          else
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='Invalid account of [GROSS PAY Charge Account].';
          end if;
      end if;
      --Add by Raymond @ 20070523 : A/C Allocation Modification
      --Check for whether users entered wrong type of A/C as the charging A/C
      /*if pkg_validate.valid_ac_typ(p_rec.pfa_sal_chrg_ctr,p_rec.pfa_sal_chrg_acc,1)!=1 then
         v_msg.extend;
         v_msg(v_msg.count).msg_type:='E';
         v_msg(v_msg.count).msg:='You have wrongly entered a PF A/C as the normal charging A/C.';
      end if;*/
      ---------------------------------------------------------------------
    end if;
    /* Checking of ERC Charge Account */
    if p_rec.pfa_ermc_chrg_acc is not null then
    v_valid_code := valid_acc_code(p_rec.pfa_ermc_chrg_acc,p_rec.pfa_fr_date,p_rec.pfa_to_date );
      if v_valid_code <> 1 then
          if v_valid_code = -1 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='[ERC Charge Account] does not exist.';
          elsif v_valid_code = -2 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='[ERC Charge Account] is not effective yet.';
          elsif v_valid_code = -3 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='W';
            v_msg(v_msg.count).msg:='From date is later than ERC charge account dormant date.';
          elsif v_valid_code = -4 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='From date is later than ERC charge account deletion date.';
          elsif v_valid_code = -5 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='W';
            v_msg(v_msg.count).msg:='To date is later than ERC charge account dormant date.';
          elsif v_valid_code = -6 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='To date is later than ERC charge account deletion date.';
          else
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='Invalid account of [ERC Charge Account].';
          end if;
      end if;
      --Add by Raymond @ 20070523 : A/C Allocation Modification
      --Check for whether users entered wrong type of A/C as the charging A/C
      /*if pkg_validate.valid_ac_typ(p_rec.pfa_ermc_chrg_ctr,p_rec.pfa_ermc_chrg_acc,2)!=1 then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          v_msg(v_msg.count).msg:='You have wrongly entered a PF A/C as the ERC A/C.';
       end if;*/
       -------------------------------------------------------------
    end if;
    /* Checking of ERAVC Charge Account */
    if p_rec.pfa_eravc_chrg_acc is not null then
      v_valid_code := valid_acc_code(p_rec.pfa_eravc_chrg_acc,p_rec.pfa_fr_date,p_rec.pfa_to_date );
      if v_valid_code <> 1 then
          if v_valid_code = -1 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='[ERAVC Charge Account] does not exist.';
          elsif v_valid_code = -2 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='[ERAVC Charge Account] is not effective yet.';
          elsif v_valid_code = -3 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='W';
            v_msg(v_msg.count).msg:='From date is later than ERAVC charge account dormant date.';
          elsif v_valid_code = -4 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='From date is later than ERAVC charge account deletion date.';
          elsif v_valid_code = -5 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='W';
            v_msg(v_msg.count).msg:='To date is later than ERAVC charge account dormant date.';
          elsif v_valid_code = -6 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='To date is later than ERAVC charge account deletion date.';
          else
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='Invalid account of [ERAVC Charge Account].';
          end if;
      end if;
      --Add by Raymond @ 20070523 : A/C Allocation Modification
      --Check for whether users entered wrong type of A/C as the charging A/C
      /*if pkg_validate.valid_ac_typ(p_rec.pfa_eravc_chrg_ctr,p_rec.pfa_eravc_chrg_acc,2)!=1 then
         v_msg.extend;
         v_msg(v_msg.count).msg_type:='E';
         v_msg(v_msg.count).msg:='You have wrongly entered a PF A/C as the ERAVC A/C.';
      end if;*/
      -----------------------------------------------------------
    end if;
    /* Matching between centre and control account */
    if not valid_ctrtyp_ctrl_ac(p_rec.pfa_sal_chrg_ctr,p_rec.pfa_sal_chrg_acc) then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='Invalid control account for [Salary Charge Centre] and [Salary Charge Account]. Please set [Charge Centre] to blank if [Charge Account] or [PFund Charge Account] is a project account. Please contact Finance for details.';
    end if;
    if not valid_ctrtyp_ctrl_ac(p_rec.pfa_ermc_chrg_ctr,p_rec.pfa_ermc_chrg_acc) then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='Invalid control account for [ERMC Charge Centre] and [ERMC Charge Account]. Please set [Charge Centre] to blank if [Charge Account] or [PFund Charge Account] is a project account. Please contact Finance for details.';
    end if;
    if not valid_ctrtyp_ctrl_ac(p_rec.pfa_eravc_chrg_ctr,p_rec.pfa_eravc_chrg_acc) then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='Invalid control account for [ERAVC Charge Centre] and [ERAVC Charge Account]. Please set [Charge Centre] to blank if [Charge Account] or [PFund Charge Account] is a project account. Please contact Finance for details.';
    end if;

    return v_msg;
  end validation; -- PFund Adjustment


  function validation(p_mode varchar2:='insert', p_rec pkg_rec.rec_pt_salhis_adj) return pkg_rec.lst_rec_msg as
  /******************************************************
   * Validation of Salary History adjustment record.
   * Use in pt_salhis_adj_form, pt_salhis_adj_dtls, pkgf_pt_salhis_adj
   ******************************************************/
    v_msg          pkg_rec.lst_rec_msg;
    v_count        pls_integer := 0;
    v_cntr_start   hr_ptcntr.pct_cntr_end%type;          -- start date of the contract
    v_cntr_end     hr_ptcntr.pct_cntr_end%type;          -- end date of the contract
    v_permit_xdate hr_staff.stf_permit_xdate%type;       -- staff's permit expiry date
    v_pyrl_mth     date := pkg_info.get_pyrl_mth;
    v_valid_code number; -- validation code for centre and account code
  begin
    v_msg:=pkg_rec.lst_rec_msg();

    /* ---------------------------- Checking of Referecne No. ---------------------------- */
    if p_rec.sha_refno is null and p_mode not like '%insert%' then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Reference No.] is a mandatory field.';
    elsif p_mode like '%insert%' then
      select sum(counts) into v_count
      from (
        select count(1) counts
        from hr_pt_pyrl_tx_his
        where prxh_tx_refno = p_rec.sha_refno
      );
      if v_count >=1 then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='The [Reference No.] ('|| p_rec.sha_refno ||') is already used in our system';
      end if;
    end if;
    /* ------------------------------ Checking of Payroll Month ------------------------------ */
    if p_rec.sha_pyrl_mth is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Payroll Month] is a mandatory field';
    else
    -- Must be previous payroll month
      if (p_rec.sha_pyrl_mth >= v_pyrl_mth) then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='Payment month must smaller than current payroll month.';
      end if;
    end if;

    /* ------------------------------ Checking of From Date ------------------------------ */
    if p_rec.sha_from_date is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Period From Date] is a mandatory field';
    /* ------------ Checking Watchman Permit Expiry Date against Payment To Date ------------ */
    elsif pkg_info.is_watchman_permit_xdate(p_rec.sha_cntr_ctr, p_rec.sha_cntr_yr, p_rec.sha_cntr_sqn, v_permit_xdate) then
      if p_rec.sha_from_date >= v_permit_xdate or v_permit_xdate is null then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='W';
        v_msg(v_msg.count).msg:='Warning: [Period From Date] is out of staff''s permit expiry date ('||v_permit_xdate||')';
      end if;
    end if;
    /* ------------------------------ Checking of To Date ------------------------------ */
    if p_rec.sha_to_date is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Period To Date] is a mandatory field';
    end if;
    /* ------------------------------ Checking of From Date/To Date period ------------------------------ */
    if p_rec.sha_from_date is not null and p_rec.sha_to_date is not null then
      if p_rec.sha_from_date>p_rec.sha_to_date then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='Payment period [To Date] must be on or after [From Date]';
      end if;
    end if;
    /* ------------------------------ Checking of Payment Code ------------------------------ */
    if p_rec.sha_paym_code is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='[Payment Code] is a mandatory field';
    end if;

    /* ------------------------------ Checking of Charge Centre Record ------------------------------ */
    /* Checking of Gross Pay Charge Centre */
    if p_rec.sha_chrg_ctr is not null then
        v_valid_code := valid_ctr_code(p_rec.sha_chrg_ctr,p_rec.sha_from_date,p_rec.sha_to_date ,'C');
        if v_valid_code != 1 and v_valid_code !=-3 and v_valid_code != -5 then
          if v_valid_code!=1 then
                if v_valid_code = -1 then
                  v_msg.extend;
                  v_msg(v_msg.count).msg_type:='E';
                  v_msg(v_msg.count).msg:='Gross Pay Charge Centre does not exist. ';
                elsif v_valid_code = -2 then
                  v_msg.extend;
                  v_msg(v_msg.count).msg_type:='E';
                  v_msg(v_msg.count).msg:='Gross Pay Charge Centre is not effective yet.';
                elsif v_valid_code = -3 then
                  v_msg.extend;
                  v_msg(v_msg.count).msg_type:='W';
                  v_msg(v_msg.count).msg:='From date is later than Gross Pay charge centre dormant date.';
                elsif v_valid_code = -4 then
                  v_msg.extend;
                  v_msg(v_msg.count).msg_type:='E';
                  v_msg(v_msg.count).msg:='From date is later than Gross Pay charge centre deletion date.';
                elsif v_valid_code = -5 then
                  v_msg.extend;
                  v_msg(v_msg.count).msg_type:='W';
                  v_msg(v_msg.count).msg:='To date is later than Gross Pay charge centre dormant date.';
                elsif v_valid_code = -6 then
                  v_msg.extend;
                  v_msg(v_msg.count).msg_type:='E';
                  v_msg(v_msg.count).msg:='To date is later than Gross Pay charge centre deletion date.';
                elsif v_valid_code = -7 then
                  v_msg.extend;
                  v_msg(v_msg.count).msg_type:='E';
                  v_msg(v_msg.count).msg:='Gross Pay Charge Centre should not be used for charging purpose.';
                else
                  v_msg.extend;
                  v_msg(v_msg.count).msg_type:='E';
                  v_msg(v_msg.count).msg:='Invaild centre of [Gross Pay Charge Centre].';
                end if;
              end if;
        end if;
     end if;
     /* ---------------------------------------------------------------
      -- Add by Raymond @ 20070525: A/C Allocation Modification
      -- Account Checking Logic:
      -- For IMC SS, Can only charge to itself
      -- For Other IMC, Can charge to itself and Project A/C
      -- For TW Centre, cannot charge to IMC Centre
      ------------------------------------------------------------------*/
      --Add by Raymond @ 20070727
      if ctrlac_chk(p_rec.sha_cntr_ctr,p_rec.sha_chrg_ctr, p_rec.sha_chrg_acc) = False then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='You must input the charge centre. (Salary Charge)';
      end if;
      if ctrlac_chk2(p_rec.sha_cntr_ctr,p_rec.sha_chrg_ctr, p_rec.sha_chrg_acc) = False then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='You must not input the charge centre. (Salary Charge)';
      end if;
      -----------------------------
      if is_imc_centre(p_rec.sha_cntr_ctr) then
         if is_ss(p_rec.sha_cntr_ctr) then
            if ss_ac_chk(p_rec.sha_cntr_ctr, p_rec.sha_chrg_ctr, p_rec.sha_chrg_acc) = false then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='You can only charge to the same centre as the serving centre only (Salary Payment)';
             end if;
          else
              if imc_ac_chk(p_rec.sha_cntr_ctr, p_rec.sha_chrg_ctr,p_rec.sha_chrg_acc) = false then
                v_msg.extend;
                v_msg(v_msg.count).msg_type:='E';
                v_msg(v_msg.count).msg:='You can only charge to the same centre as the serving centre or project A/C (Salary Payment)';
              end if;
          end if;
          if imc_ctrlac_chk(p_rec.sha_cntr_ctr, p_rec.sha_chrg_ctr, p_rec.sha_chrg_acc) = false then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='Charge centre cannot be blanked for this Salary Payment A/C';
          end if;
      else
        -----------------------------------------------------------------------------
        -- Conrad Kwong @ 2018-10-04, Form# HRIS-19007, change for KG/NS centre
        if is_kgns_centre_YN(p_rec.sha_cntr_ctr) = 'Y' then
          if kgns_ac_chk(p_rec.sha_cntr_ctr, p_rec.sha_chrg_ctr, p_rec.sha_chrg_acc) = false then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='You can only charge to TW A/C or correct project A/C (Salary Payment)';
          end if;
          if kgns_ctrlac_chk(p_rec.sha_cntr_ctr, p_rec.sha_chrg_ctr, p_rec.sha_chrg_acc) = false then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='Charge centre cannot be blank for Salary Payment A/C';
          end if;
        else
        -----------------------------------------------------------------------------
          if tw_ac_chk(p_rec.sha_cntr_ctr, p_rec.sha_chrg_ctr,p_rec.sha_chrg_acc) = false then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            -----------------------------------------------------------------------------
            -- Conrad Kwong @ 2018-10-04, Form# HRIS-19007, change for KG/NS centre
            --v_msg(v_msg.count).msg:='You can only charge to the TW centre or project A/C (Salary Payment)';
            v_msg(v_msg.count).msg:='You can only charge to TW A/C, KG/NS A/C or correct project A/C (Salary Payment)';
            -----------------------------------------------------------------------------
          end if;
          if tw_ctrlac_chk(p_rec.sha_cntr_ctr, p_rec.sha_chrg_ctr, p_rec.sha_chrg_acc) = false then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='Charge centre cannot be blanked for this Salary Payment A/C';
          end if;
        -----------------------------------------------------------------------------
        -- Conrad Kwong @ 2018-10-04, Form# HRIS-19007, change for KG/NS centre
        end if;
        -----------------------------------------------------------------------------
      end if;
      if chk_proj_ac(p_rec.sha_chrg_acc, p_rec.sha_chrg_ctr) = false then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='No need to input charge centre for this Salary Payment A/C';
      end if;
      -- End of Addition ----------------------------------------------------

    /* Checking of ERMC Charge Centre */
    if p_rec.sha_ermc_chrg_ctr is not null then
      v_valid_code := valid_ctr_code(p_rec.sha_ermc_chrg_ctr,p_rec.sha_from_date,p_rec.sha_to_date ,'C');
      if v_valid_code != 1 then
              if v_valid_code = -1 then
                v_msg.extend;
                v_msg(v_msg.count).msg_type:='E';
                v_msg(v_msg.count).msg:='ERC Charge Centre does not exist. ';
              elsif v_valid_code = -2 then
                v_msg.extend;
                v_msg(v_msg.count).msg_type:='E';
                v_msg(v_msg.count).msg:='ERC Charge Centre is not effective yet.';
              elsif v_valid_code = -3 then
                v_msg.extend;
                v_msg(v_msg.count).msg_type:='W';
                v_msg(v_msg.count).msg:='From date is later than ERC charge centre dormant date.';
              elsif v_valid_code = -4 then
                v_msg.extend;
                v_msg(v_msg.count).msg_type:='E';
                v_msg(v_msg.count).msg:='From date is later than ERC charge centre deletion date.';
              elsif v_valid_code = -5 then
                v_msg.extend;
                v_msg(v_msg.count).msg_type:='W';
                v_msg(v_msg.count).msg:='To date is later than ERC charge centre dormant date.';
              elsif v_valid_code = -6 then
                v_msg.extend;
                v_msg(v_msg.count).msg_type:='E';
                v_msg(v_msg.count).msg:='To date is later than ERC charge centre deletion date.';
              elsif v_valid_code = -7 then
                v_msg.extend;
                v_msg(v_msg.count).msg_type:='E';
                v_msg(v_msg.count).msg:='ERC Charge Centre should not be used for charging purpose.';
              else
                v_msg.extend;
                v_msg(v_msg.count).msg_type:='E';
                v_msg(v_msg.count).msg:='Invaild centre of [ERC Charge Centre].';
              end if;
        end if;
      end if;
     /* ---------------------------------------------------------------
      -- Add by Raymond @ 20070525: A/C Allocation Modification
      -- Account Checking Logic:
      -- For IMC SS, Can only charge to itself
      -- For Other IMC, Can charge to itself and Project A/C
      -- For TW Centre, cannot charge to IMC Centre
      ------------------------------------------------------------------*/
      --Add by Raymond @ 20070727
      if ctrlac_chk(p_rec.sha_cntr_ctr,p_rec.sha_ermc_chrg_ctr, p_rec.sha_ermc_chrg_acc) = False then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='You must input the charge centre. (ERMC)';
      end if;
      if ctrlac_chk2(p_rec.sha_cntr_ctr,p_rec.sha_ermc_chrg_ctr, p_rec.sha_ermc_chrg_acc) = False then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='You must not input the charge centre. (ERMC)';
      end if;
      -----------------------------
      if is_imc_centre(p_rec.sha_cntr_ctr) then
         if is_ss(p_rec.sha_cntr_ctr) then
            if ss_ac_chk(p_rec.sha_cntr_ctr, p_rec.sha_ermc_chrg_ctr,p_rec.sha_ermc_chrg_acc) = false then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='You can only charge to the same centre as the serving centre only (ERC Payment)';
             end if;
          else
              if imc_ac_chk(p_rec.sha_cntr_ctr, p_rec.sha_ermc_chrg_ctr,p_rec.sha_ermc_chrg_acc) = false then
                v_msg.extend;
                v_msg(v_msg.count).msg_type:='E';
                v_msg(v_msg.count).msg:='You can only charge to the same centre as the serving centre or project A/C (ERC Payment)';
              end if;
          end if;
          if imc_ctrlac_chk(p_rec.sha_cntr_ctr, p_rec.sha_ermc_chrg_ctr, p_rec.sha_ermc_chrg_acc) = false then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='Charge centre cannot be blanked for this ERC A/C';
          end if;
      else
        -----------------------------------------------------------------------------
        -- Conrad Kwong @ 2018-10-04, Form# HRIS-19007, change for KG/NS centre
        if is_kgns_centre_YN(p_rec.sha_cntr_ctr) = 'Y' then
          if kgns_ac_chk(p_rec.sha_cntr_ctr, p_rec.sha_ermc_chrg_ctr, p_rec.sha_ermc_chrg_acc) = false then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='You can only charge to TW A/C or correct project A/C (ERC Payment)';
          end if;
          if kgns_ctrlac_chk(p_rec.sha_cntr_ctr, p_rec.sha_ermc_chrg_ctr, p_rec.sha_ermc_chrg_acc) = false then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='Charge centre cannot be blank for ERC A/C';
          end if;
        else
        -----------------------------------------------------------------------------
          if tw_ac_chk(p_rec.sha_cntr_ctr, p_rec.sha_ermc_chrg_ctr,p_rec.sha_ermc_chrg_acc) = false then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            -----------------------------------------------------------------------------
            -- Conrad Kwong @ 2018-10-04, Form# HRIS-19007, change for KG/NS centre
            --v_msg(v_msg.count).msg:='You can only charge to the TW centre or project A/C (ERC Payment)';
            v_msg(v_msg.count).msg:='You can only charge to TW A/C, KG/NS A/C or correct project A/C (ERC Payment)';
            -----------------------------------------------------------------------------
          end if;
          if tw_ctrlac_chk(p_rec.sha_cntr_ctr, p_rec.sha_ermc_chrg_ctr, p_rec.sha_ermc_chrg_acc) = false then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='Charge centre cannot be blanked for this ERC A/C';
          end if;
        -----------------------------------------------------------------------------
        -- Conrad Kwong @ 2018-10-04, Form# HRIS-19007, change for KG/NS centre
        end if;
        -----------------------------------------------------------------------------
      end if;
      if chk_proj_ac(p_rec.sha_ermc_chrg_acc, p_rec.sha_ermc_chrg_ctr) = false then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='No need to input charge centre for this ERC A/C';
      end if;
      -- End of Addition ----------------------------------------------------

     /* Checking of ERAVC Charge Centre */
     if p_rec.sha_eravc_chrg_ctr is not null then
      v_valid_code := valid_ctr_code(p_rec.sha_eravc_chrg_ctr,p_rec.sha_from_date,p_rec.sha_to_date ,'C');
    if v_valid_code != 1 and v_valid_code !=-3 and v_valid_code != -5 then
      if v_valid_code!=1 then
            if v_valid_code = -1 then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='ERAVC Charge Centre does not exist. ';
            elsif v_valid_code = -2 then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='ERAVC Charge Centre is not effective yet.';
            elsif v_valid_code = -3 then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='W';
              v_msg(v_msg.count).msg:='From date is later than ERAVC charge centre dormant date.';
            elsif v_valid_code = -4 then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='From date is later than ERAVC charge centre deletion date.';
            elsif v_valid_code = -5 then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='W';
              v_msg(v_msg.count).msg:='To date is later than ERAVC charge centre dormant date.';
            elsif v_valid_code = -6 then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='To date is later than ERAVC charge centre deletion date.';
            elsif v_valid_code = -7 then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='ERAVC Charge Centre should not be used for charging purpose.';
            else
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='Invaild centre of [ERAVC Charge Centre].';
            end if;
          end if;
      end if;
    end if;
    /* ---------------------------------------------------------------
    -- Add by Raymond @ 20070525: A/C Allocation Modification
    -- Account Checking Logic:
    -- For IMC SS, Can only charge to itself
    -- For Other IMC, Can charge to itself and Project A/C
    -- For TW Centre, cannot charge to IMC Centre
    ------------------------------------------------------------------*/
      --Add by Raymond @ 20070727
      if ctrlac_chk(p_rec.sha_cntr_ctr,p_rec.sha_eravc_chrg_ctr, p_rec.sha_eravc_chrg_acc) = False then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='You must input the charge centre. (ERAVC)';
      end if;
      if ctrlac_chk2(p_rec.sha_cntr_ctr,p_rec.sha_eravc_chrg_ctr, p_rec.sha_eravc_chrg_acc) = False then
        v_msg.extend;
        v_msg(v_msg.count).msg_type:='E';
        v_msg(v_msg.count).msg:='You must not input the charge centre. (ERAVC)';
      end if;
      -----------------------------
    if is_imc_centre(p_rec.sha_cntr_ctr) then
       if is_ss(p_rec.sha_cntr_ctr) then
          if ss_ac_chk(p_rec.sha_cntr_ctr, p_rec.sha_eravc_chrg_ctr, p_rec.sha_eravc_chrg_acc) = false then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='You can only charge to the same centre as the serving centre only (ERAVC Payment)';
           end if;
        else
            if imc_ac_chk(p_rec.sha_cntr_ctr, p_rec.sha_eravc_chrg_ctr,p_rec.sha_eravc_chrg_acc) = false then
              v_msg.extend;
              v_msg(v_msg.count).msg_type:='E';
              v_msg(v_msg.count).msg:='You can only charge to the same centre as the serving centre or project A/C (ERAVC Payment)';
            end if;
        end if;
        if imc_ctrlac_chk(p_rec.sha_cntr_ctr, p_rec.sha_eravc_chrg_ctr, p_rec.sha_eravc_chrg_acc) = false then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          v_msg(v_msg.count).msg:='Charge centre cannot be blanked for this ERAVC A/C';
        end if;
    else
      -----------------------------------------------------------------------------
      -- Conrad Kwong @ 2018-10-04, Form# HRIS-19007, change for KG/NS centre
      if is_kgns_centre_YN(p_rec.sha_cntr_ctr) = 'Y' then
        if kgns_ac_chk(p_rec.sha_cntr_ctr, p_rec.sha_eravc_chrg_ctr, p_rec.sha_eravc_chrg_acc) = false then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          v_msg(v_msg.count).msg:='You can only charge to TW A/C or correct project A/C (ERAVC Payment)';
        end if;
        if kgns_ctrlac_chk(p_rec.sha_cntr_ctr, p_rec.sha_eravc_chrg_ctr, p_rec.sha_eravc_chrg_acc) = false then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          v_msg(v_msg.count).msg:='Charge centre cannot be blank for ERAVC A/C';
        end if;
      else
      -----------------------------------------------------------------------------
        if tw_ac_chk(p_rec.sha_cntr_ctr, p_rec.sha_eravc_chrg_ctr,p_rec.sha_eravc_chrg_acc) = false then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          -----------------------------------------------------------------------------
          -- Conrad Kwong @ 2018-10-04, Form# HRIS-19007, change for KG/NS centre
          --v_msg(v_msg.count).msg:='You can only charge to the TW centre or project A/C (ERAVC Payment)';
          v_msg(v_msg.count).msg:='You can only charge to TW A/C, KG/NS A/C or correct project A/C (ERAVC Payment)';
          -----------------------------------------------------------------------------
        end if;
        if tw_ctrlac_chk(p_rec.sha_cntr_ctr, p_rec.sha_eravc_chrg_ctr, p_rec.sha_eravc_chrg_acc) = false then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          v_msg(v_msg.count).msg:='Charge centre cannot be blanked for this ERAVC A/C';
        end if;
      -----------------------------------------------------------------------------
      -- Conrad Kwong @ 2018-10-04, Form# HRIS-19007, change for KG/NS centre
      end if;
      -----------------------------------------------------------------------------
    end if;
    if chk_proj_ac(p_rec.sha_eravc_chrg_acc, p_rec.sha_eravc_chrg_ctr) = false then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='No need to input charge centre for this ERAVC A/C';
    end if;
    -- End of Addition ----------------------------------------------------

    /* ------------------------------ Checking of Charge Account Record ------------------------------ */
    /* Checking of Charge Account */
    if p_rec.sha_chrg_acc is null and p_rec.sha_ermc_chrg_acc is null and p_rec.sha_eravc_chrg_acc is null then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='Invalid of [Charge Account]. At least input one charge account record.';
    elsif (p_rec.sha_chrg_acc is not null and not is_account(p_rec.sha_chrg_acc)) or
          (p_rec.sha_ermc_chrg_acc is not null and not is_account(p_rec.sha_ermc_chrg_acc)) or
          (p_rec.sha_eravc_chrg_acc is not null and not is_account(p_rec.sha_eravc_chrg_acc))
    then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='Invalid account of [Charge Account].';
    end if;

    /* Checking of Gross Pay Charge Account */
    if p_rec.sha_chrg_acc is not null then
      v_valid_code := valid_acc_code(p_rec.sha_chrg_acc,p_rec.sha_from_date,p_rec.sha_to_date );
      if v_valid_code <> 1 then
          if v_valid_code = -1 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='[Gross Pay Charge Account] does not exist.';
          elsif v_valid_code = -2 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='[Gross Pay Charge Account] is not effective yet.';
          elsif v_valid_code = -3 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='W';
            v_msg(v_msg.count).msg:='From date is later than gross pay charge account dormant date.';
          elsif v_valid_code = -4 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='From date is later than gross pay charge account deletion date.';
          elsif v_valid_code = -5 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='W';
            v_msg(v_msg.count).msg:='To date is later than gross pay charge account dormant date.';
          elsif v_valid_code = -6 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='To date is later than gross pay charge account deletion date.';
          else
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='Invalid account of [GROSS PAY Charge Account].';
          end if;
      end if;
      --Add by Raymond @ 20070523 : A/C Allocation Modification
      --Check for whether users has entered the correct type of A/C as the charging A/C
      /*if pkg_validate.valid_ac_typ(p_rec.sha_chrg_ctr,p_rec.sha_chrg_acc,1)!=1 then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          v_msg(v_msg.count).msg:='You have wrongly entered a PF A/C as the normal charging A/C.';
       end if;*/
       -----------------------------------------------------------
    end if;
    /* Checking of ERC Charge Account */
    if p_rec.sha_ermc_chrg_acc is not null then
      v_valid_code := valid_acc_code(p_rec.sha_ermc_chrg_acc,p_rec.sha_from_date,p_rec.sha_to_date );
      if v_valid_code <> 1 then
          if v_valid_code = -1 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='[ERC Charge Account] does not exist.';
          elsif v_valid_code = -2 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='[ERC Charge Account] is not effective yet.';
          elsif v_valid_code = -3 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='W';
            v_msg(v_msg.count).msg:='From date is later than ERC charge account dormant date.';
          elsif v_valid_code = -4 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='From date is later than ERC charge account deletion date.';
          elsif v_valid_code = -5 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='W';
            v_msg(v_msg.count).msg:='To date is later than ERC charge account dormant date.';
          elsif v_valid_code = -6 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='To date is later than ERC charge account deletion date.';
          else
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='Invalid account of [ERC Charge Account].';
          end if;
      end if;
      --Add by Raymond @ 20070523 : A/C Allocation Modification
      --Check for whether users has entered the correct type of A/C as the charging A/C
      /*if pkg_validate.valid_ac_typ(p_rec.sha_ermc_chrg_ctr,p_rec.sha_ermc_chrg_acc,2)!=1 then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          v_msg(v_msg.count).msg:='You have wrongly entered a PF A/C as the ERC A/C.';
       end if;*/
       -----------------------------------------------------------
    end if;
    /* Checking of ERAVC Charge Account */
    if p_rec.sha_eravc_chrg_acc is not null then
      v_valid_code := valid_acc_code(p_rec.sha_eravc_chrg_acc,p_rec.sha_from_date,p_rec.sha_to_date );
      if v_valid_code <> 1 then
          if v_valid_code = -1 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='[ERAVC Charge Account] does not exist.';
          elsif v_valid_code = -2 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='[ERAVC Charge Account] is not effective yet.';
          elsif v_valid_code = -3 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='W';
            v_msg(v_msg.count).msg:='From date is later than ERAVC charge account dormant date.';
          elsif v_valid_code = -4 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='From date is later than ERAVC charge account deletion date.';
          elsif v_valid_code = -5 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='W';
            v_msg(v_msg.count).msg:='To date is later than ERAVC charge account dormant date.';
          elsif v_valid_code = -6 then
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='To date is later than ERAVC charge account deletion date.';
          else
            v_msg.extend;
            v_msg(v_msg.count).msg_type:='E';
            v_msg(v_msg.count).msg:='Invalid account of [ERAVC Charge Account].';
          end if;
      end if;
      --Add by Raymond @ 20070523 : A/C Allocation Modification
      --Check for whether users has entered the correct type of A/C as the charging A/C
      /*if pkg_validate.valid_ac_typ(p_rec.sha_eravc_chrg_ctr,p_rec.sha_eravc_chrg_acc,2)!=1 then
          v_msg.extend;
          v_msg(v_msg.count).msg_type:='E';
          v_msg(v_msg.count).msg:='You have wrongly entered a PF A/C as the ERAVC A/C.';
       end if;*/
       -----------------------------------------------------------
    end if;


    /* Checking of Charge Amount */
    if p_rec.sha_gross_amt is null and
       p_rec.sha_eemc_amt is null and p_rec.sha_eeavc_amt is null and
       p_rec.sha_ermc_amt is null and p_rec.sha_eravc_amt is null
    then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='Invalid of [Charge Amount]. At least input one charge amount record.';
    end if;
    /* Matching between centre and control account */
    if not valid_ctrtyp_ctrl_ac(p_rec.sha_chrg_ctr,p_rec.sha_chrg_acc) then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='Invalid control account for [Salary Charge Centre] and [Salary Charge Account]. Please set [Charge Centre] to blank if [Charge Account] or [PFund Charge Account] is a project account. Please contact Finance for details.';
    end if;
    if not valid_ctrtyp_ctrl_ac(p_rec.sha_ermc_chrg_ctr,p_rec.sha_ermc_chrg_acc) then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='Invalid control account for [ERMC Charge Centre] and [ERMC Charge Account]. Please set [Charge Centre] to blank if [Charge Account] or [PFund Charge Account] is a project account. Please contact Finance for details.';
    end if;
    if not valid_ctrtyp_ctrl_ac(p_rec.sha_eravc_chrg_ctr,p_rec.sha_eravc_chrg_acc) then
      v_msg.extend;
      v_msg(v_msg.count).msg_type:='E';
      v_msg(v_msg.count).msg:='Invalid control account for [ERAVC Charge Centre] and [ERAVC Charge Account]. Please set [Charge Centre] to blank if [Charge Account] or [PFund Charge Account] is a project account. Please contact Finance for details.';
    end if;

    return v_msg;
  end validation; -- Salary History Adjustment

-----------------------------------------------------------------------
-- Author         : Raymond Ng
-- Create Date    : 2007-05-25
-- function       : Check whether the centre is IMC SS or not
-----------------------------------------------------------------------
function is_ss(p_ctr_code hr_centre.ctr_code%type) return boolean is
   v_count number := 0;
begin
   select count(*) into v_count
   from hr_centre
   ---------------------------------------------------------------------------------
   -- Conrad Kwong @ 2018-10-04, Form# HRIS-19007, change for KG/NS centre
   inner join hr_imc on imc_code = ctr_imc_code
   ---------------------------------------------------------------------------------
   where ctr_code = p_ctr_code
         ---------------------------------------------------------------------------------
         -- Conrad Kwong @ 2018-10-04, Form# HRIS-19007, change for KG/NS centre
         --and ctr_imc_code is not null
         and imc_rectype = 'IM'
         ---------------------------------------------------------------------------------
         and ctr_svs_code = '401010';

   if v_count = 0 then
      return false;
   else
      return true;
   end if;
end;

-----------------------------------------------------------------------
-- Author         : Raymond Ng
-- Create Date    : 2007-05-25
-- function       : IMC SS A/C Checking
-----------------------------------------------------------------------
function ss_ac_chk(p_serv_ctr hr_centre.ctr_code%type, p_chrg_ctr hr_centre.ctr_code%type,p_ac varchar2) return boolean is
   v_result boolean := true;
   v_imc_code1 hr_centre.ctr_imc_code%type;
   v_imc_code2 hr_centre.ctr_imc_code%type;
begin
   begin
     select ctr_imc_code
       into v_imc_code1
       from hr_centre
      where ctr_code = p_serv_ctr;
   exception
      when others then
        v_result := false;
   end;

   if p_chrg_ctr is not null then
     begin
       select ctr_imc_code
         into v_imc_code2
         from hr_centre
        where ctr_code = p_chrg_ctr;
     exception
        when others then
          v_result := false;
     end;
   end if   ;

   if v_result = true then
     if p_chrg_ctr is not null then
       if v_imc_code1 != v_imc_code2 then
          v_result := false;
       end if;
     end if;

     if substr(p_ac,1,2) != v_imc_code1 then
        v_result := false;
     end if;
   end if;
   return v_result;
end;

-----------------------------------------------------------------------
-- Author         : Raymond Ng
-- Create Date    : 2007-05-25
-- function       : Other IMC CTR A/C Checking
-----------------------------------------------------------------------
function imc_ac_chk(p_serv_ctr hr_centre.ctr_code%type, p_chrg_ctr hr_centre.ctr_code%type, p_ac varchar2) return boolean is
   v_result boolean := true;
   v_imc_code1 hr_centre.ctr_imc_code%type;
   v_imc_code2 hr_centre.ctr_imc_code%type;
begin
   begin
     select ctr_imc_code
       into v_imc_code1
       from hr_centre
      where ctr_code = p_serv_ctr;
   exception
      when others then
        v_result := false;
   end;

   if p_chrg_ctr is not null then
     begin
       select ctr_imc_code
         into v_imc_code2
         from hr_centre
        where ctr_code = p_chrg_ctr;
     exception
        when others then
          v_result := false;
     end;
   end if;

   if v_result = true then
     if p_chrg_ctr is not null then
       if v_imc_code1 != v_imc_code2 then
          v_result := false;
       end if;
     end if;

     if substr(p_ac,1,2) != '11' and substr(p_ac,1,2) != v_imc_code1 then
        v_result := false;
     end if;
   end if;

   return v_result;
end;


-----------------------------------------------------------------------
-- Author         : Raymond Ng
-- Create Date    : 2007-05-25
-- function       : TW A/C Check
-----------------------------------------------------------------------
function tw_ac_chk(p_serv_ctr hr_centre.ctr_code%type, p_chrg_ctr hr_centre.ctr_code%type, p_ac varchar2) return boolean is
   v_result boolean := true;
begin
   if p_chrg_ctr is not null then
     if is_imc_centre(p_chrg_ctr) then
        v_result := false;
     -----------------------------------------------------------------------------
     -- Conrad Kwong @ 2018-10-04, Form# HRIS-19007, change for KG/NS centre
     --end if;
     elsif is_kgns_centre_yn(p_chrg_ctr) = 'Y' then
       declare
         v_kgns_code hr_centre.ctr_imc_code%type := null;
       begin
         select ctr_imc_code
           into v_kgns_code
           from hr_centre
          where ctr_code = p_chrg_ctr;

          if v_kgns_code <> substr(p_ac,1,2) then
            v_result := false;
          end if;
       end;
     else
       if not is_number(substr(p_ac,1,2)) then
          v_result := false;
       end if;
     end if;
     -----------------------------------------------------------------------------
   end if;

   /*if v_result = true then
     if not is_number(substr(p_ac,1,2)) then
        v_result := false;
     end if;
   end if;*/
   return v_result;
end;

FUNCTION is_number(p_var LONG) RETURN BOOLEAN IS
  num NUMBER;
BEGIN
  num := to_number(p_var);
  RETURN TRUE;
EXCEPTION
  WHEN OTHERS THEN
    RETURN FALSE;
END;

----------------------------------------------------------------------------------------
-- Author         : Raymond
-- Create Date    : 2007-06-13
-- function       : TW Ctrl A/C Check based on New Income / Exp. Ctrl A/C field from FAS
-----------------------------------------------------------------------------------------
FUNCTION tw_ctrlac_chk(p_serv_ctr hr_centre.ctr_code%TYPE,
                       p_chrg_ctr hr_centre.ctr_code%TYPE,
                       p_ac       VARCHAR2) RETURN BOOLEAN IS
  v_result   BOOLEAN := TRUE;
  v_inc_ctl  hr_centre.ctr_inc_ctl%TYPE;
  v_exp_ctl  hr_centre.ctr_exp_ctl%TYPE;
  v_eff_date hr_centre.ctr_eff_date%type;
BEGIN
  -----------------------------------------------------------------------------
  -- Conrad Kwong @ 2018-10-04, Form# HRIS-19007, change for KG/NS centre
  /*BEGIN
    SELECT ctr_inc_ctl, ctr_exp_ctl, ctr_eff_date
      INTO v_inc_ctl, v_exp_ctl, v_eff_date
      FROM hr_centre
     WHERE ctr_code = p_serv_ctr;
  EXCEPTION
    WHEN OTHERS THEN
      v_result := FALSE;
  END;

  IF v_result = TRUE THEN
    IF substr(p_ac, 1, 4) IN (substr(v_inc_ctl, 1, 4), substr(v_exp_ctl, 1, 4)) AND
       p_chrg_ctr IS NULL THEN
      v_result := FALSE;
    END IF;

  END IF;*/
  IF need_chrgctr(p_ac => p_ac) = 'Y' AND p_chrg_ctr IS NULL THEN
     v_result := FALSE;
  ELSE
     v_result := TRUE;
  END IF;

  RETURN v_result;
END;

----------------------------------------------------------------------------------------
-- Author         : Raymond
-- Create Date    : 2007-06-13
-- function       : IMC Ctrl A/C Check based on New Income / Exp. Ctrl A/C field from FAS
-----------------------------------------------------------------------------------------
FUNCTION imc_ctrlac_chk(p_serv_ctr hr_centre.ctr_code%TYPE,
                        p_chrg_ctr hr_centre.ctr_code%TYPE,
                        p_ac       VARCHAR2) RETURN BOOLEAN IS
  v_result   BOOLEAN := TRUE;
  v_inc_ctl  hr_centre.ctr_inc_ctl%TYPE;
  v_exp_ctl  hr_centre.ctr_exp_ctl%TYPE;
BEGIN
  -----------------------------------------------------------------------------
  -- Conrad Kwong @ 2018-10-04, Form# HRIS-19007, change for KG/NS centre
  /*BEGIN
    SELECT ctr_inc_ctl, ctr_exp_ctl
      INTO v_inc_ctl, v_exp_ctl
      FROM hr_centre
     WHERE ctr_code = p_serv_ctr;
  EXCEPTION
    WHEN OTHERS THEN
      v_result := FALSE;
  END;

  IF v_result = TRUE THEN
    IF substr(p_ac, 1, 3) IN (substr(v_inc_ctl, 1, 3), substr(v_exp_ctl, 1, 3)) AND
       p_chrg_ctr IS NULL THEN
      v_result := FALSE;
    END IF;
  END IF;*/
  IF need_chrgctr(p_ac => p_ac) = 'Y' AND p_chrg_ctr IS NULL THEN
     v_result := FALSE;
  ELSE
     v_result := TRUE;
  END IF;

  RETURN v_result;
END;

----------------------------------------------------------------------------------------
-- Author         : Raymond
-- Create Date    : 2007-06-13
-- function       : Proj A/C no need to input charge centre
-----------------------------------------------------------------------------------------
FUNCTION chk_proj_ac(p_ac VARCHAR2, p_chrg_ctr hr_centre.ctr_code%type) RETURN BOOLEAN IS
  v_result   BOOLEAN := TRUE;
BEGIN
  if to_char(substr(p_ac,1,2)) in ('11','16','34','92') or to_char(substr(p_ac,1,1)) in ('6') then
     if p_chrg_ctr is not null then
        v_result := false;
     end if;
  end if;
  return v_result;
END;

FUNCTION need_chrgctr(p_ac VARCHAR2) RETURN CHAR IS
  v_count  NUMBER := 0;
  v_first_three_digit char(4);
BEGIN
  -------------------------------------------------------------------------------------
  -- Conrad Kwong @ 2018-10-04, Form# HRIS-19007, change for KG/NS centre
  --        replace this function with newly crreated hrp.need_chrg_ctr
  /*v_first_three_digit:=substr(p_ac , 1, 3);
  BEGIN
    -- Check Inc / Exp. A/C in HR_CENTRE
    IF pkg_validate.is_number(substr(p_ac, 1, 2)) = TRUE then
      IF v_first_three_digit IN ('450','550') THEN
        RETURN 'Y';
      ELSE
        SELECT COUNT(*)
          INTO v_count
          FROM dual
         WHERE substr(p_ac, 1, 4) IN
               (SELECT DISTINCT substr(ac, 1, 4)
                  FROM (SELECT ctr.ctr_inc_ctl ac
                          FROM hr_centre ctr
                         WHERE ctr_imc_code IS NULL
                        UNION
                        SELECT ctr.ctr_exp_ctl ac
                          FROM hr_centre ctr
                         WHERE ctr_imc_code IS NULL));
      END IF;
    ELSE
      SELECT COUNT(*)
        INTO v_count
        FROM dual
       WHERE substr(p_ac, 1, 3) IN
             (SELECT DISTINCT substr(ac, 1, 3)
                FROM (SELECT ctr.ctr_inc_ctl ac
                        FROM hr_centre ctr
                       WHERE ctr_imc_code IS NOT NULL
                      UNION
                      SELECT ctr.ctr_exp_ctl ac
                        FROM hr_centre ctr
                       WHERE ctr_imc_code IS NOT NULL));
    END IF;

  EXCEPTION
    WHEN no_data_found THEN
      v_count := 0;

    WHEN OTHERS THEN
      RETURN NULL;

  END;

  IF v_count > 0 THEN
     RETURN 'Y';
  ELSE
     RETURN 'N';
  END IF;*/
  SELECT COUNT(*)
    INTO v_count
    FROM hr_ac_name
   WHERE acn_ac_code = p_ac
     AND acn_recurr_type IN ('I', 'E');

  IF v_count > 0 THEN
    RETURN 'Y';
  ELSE
    RETURN 'N';
  END IF;

EXCEPTION
  WHEN no_data_found THEN
    RETURN 'N';
  -------------------------------------------------------------------------------------

END;


FUNCTION ctrlac_chk(p_serv_ctr hr_centre.ctr_code%TYPE,p_chrg_ctr hr_centre.ctr_code%TYPE,p_ac VARCHAR2) RETURN BOOLEAN IS
  v_result BOOLEAN := TRUE;
BEGIN
  IF need_chrgctr(p_ac) = 'Y' AND p_chrg_ctr IS NULL THEN
    v_result := FALSE;
  END IF;

  RETURN v_result;
END;


FUNCTION ctrlac_chk2(p_serv_ctr hr_centre.ctr_code%TYPE,p_chrg_ctr hr_centre.ctr_code%TYPE,p_ac VARCHAR2) RETURN BOOLEAN IS
  v_result BOOLEAN := TRUE;
BEGIN

  IF need_chrgctr(p_ac) = 'N' AND p_chrg_ctr IS NOT NULL THEN
    v_result := FALSE;
  END IF;

  RETURN v_result;
END;
end pkg_validate;
