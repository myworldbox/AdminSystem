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
    end if;

    end if;
    
    end if;
    
    
    ------------------------------------------------------------------------------------------
    -- Conrad Kwong @ 2025-06-10, Form# HRIS-250xx, modify for eMPF
    --if (p_mode ='insert') then 
    if p_mode <> 'display' then 
      --if p_rec.stf_phone1areacode is null OR p_rec.stf_phone1 is null or p_rec.stf_email is null  then

      end if;
    end if;
    ------------------------------------------------------------------------------------------

   -- Angel Chan modofied @ 20250217 ---------------------------------------------------------
  -------------------------------------------------------------------------------------------------------
  -- Conrad Kwong @ 2017-04-10, Form# HRISPT-17002, Validate Chinese Character
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
    --------------- Not allow date of birth being a future date -----------

    --------------- Show warning if the staff is under 15 years old ------------------

    end if;

    ----------- Staff gender must be entered ------------------
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