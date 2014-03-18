(function (win, $) {
    if (typeof SPClientTemplates === 'undefined') {
        return;
    }

    win.BS = win.BS || {};
    win.BS.Fields = win.BS.Fields || {};
    win.BS.Fields.BPP = {};
    var field = win.BS.Fields.BPP;

    var context, thousandsSeperator, decimalSeperator;

    var titleFieldId = 'Title_fa564e0f-0c70-4ab9-b863-0177e6ddd247_$TextField',
        dueDateFieldId = 'bspDueDate_92fa0af2-ad10-44f3-8532-3ede6dd9490e_$DateTimeFieldDate',
        participantsDivId = 'bspParticipants_302c57ea-9093-492d-b6d6-43d64691ebfd_$ClientPeoplePicker',
        earningsFieldId = 'bspEarnings_7efd997c-fa52-474b-bafe-86dab7bf185e_$CurrencyField',
        expensesFieldId = 'bspExpenses_54eba282-5070-49c1-be9b-e44ff5265e57_$CurrencyField',
        ruleEngineFieldId = 'bspRuleEngine_eba2d59a-13b5-4395-b5b8-0cbcd031af45_$DropDownChoice',
        ruleGroupFieldId = 'bspRuleGroup_e468f168-a608-4e26-93b2-8cfcf5816b13_$TextField';

    var bonusPerParticipantFieldId = 'bspBonusPerParticipantField';
    var bonusPerParticipantValue;

    var ruleEngines = {
        "JavaScript": 0,
        "NxBRE": 1,
        "SRE (Simple Rule Engine)": 2
    };

    field.calculateBonus = function () {
        var titleField = $('#' + titleFieldId.replace(/\$/g, '\\\$')),
            dueDateField = $('#' + dueDateFieldId.replace(/\$/g, '\\\$')),
            participantsDiv = $('#' + participantsDivId.replace(/\$/g, '\\\$')),
            earningsField = $('#' + earningsFieldId.replace(/\$/g, '\\\$')),
            expensesField = $('#' + expensesFieldId.replace(/\$/g, '\\\$')),
            ruleEngineField = $('#' + ruleEngineFieldId.replace(/\$/g, '\\\$')),
            ruleGroupField = $('#' + ruleGroupFieldId.replace(/\$/g, '\\\$'));


        setValue('calculating...');

        var peoplePicker = SPClientPeoplePicker.SPClientPeoplePickerDict[participantsDiv[0].id];
        var selectedParticipants = peoplePicker.GetAllUserInfo();
        var participants = [];

        Array.forEach(selectedParticipants, function (participant) {
            participants.push(participant.DisplayText);
        });

        var earnings = earningsField.val();
        var expenses = expensesField.val();
        var ruleEngine = ruleEngines[ruleEngineField.val()];
        var ruleGroup = ruleGroupField.val();

        earnings = parseFloat(earnings.replace(thousandsSeperator, '')),
        expenses = expenses ? parseFloat(expenses.replace(thousandsSeperator, '')) : 0;

        if (ruleEngine === 0) {
            
            if (ruleGroup) {

            } else {
                setValue(getParsedValue((earnings - expenses) / participants.length));
            }
        }
        else {
            var values = {
                Title: titleField.val(),
                DueDate: dueDateField.val(),
                Participants: participants,
                Earnings: earnings,
                Expenses: expenses,
                RuleEngine: ruleEngine,
                RuleGroup: ruleGroup,
                BonusPerParticipant: 0
            };

            $.ajax({
                type: 'POST',
                url: L_Menu_BaseUrl + '/_layouts/15/BonusScheme/ASPX/Executor.aspx',
                data: { project: JSON.stringify(values) }
            }).done(function (returnedJSON) {
                if (returnedJSON.Succeeded) {
                    setValue(getParsedValue(returnedJSON.Value));
                } else {
                    setValue('Error');
                    console.log(returnedJSON.Message);
                }
            }).fail(function (jqXHR, message) {
                setValue('Error');
                console.log(message);
            });
        }
    };

    var setValue = function (value) {
        bonusPerParticipantValue = value;
        var bbpField = $('#' + bonusPerParticipantFieldId);
        bbpField.val(value);
    };

    var getParsedValue = function (value) {
        var valueType = typeof (value);

        switch (valueType) {
            case 'number': { return value.toFixed(2).replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1" + thousandsSeperator); }
            default: { return parseFloat(value).toFixed(2).replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1" + thousandsSeperator); }
        }
    };

    field.View = function (ctx) {
        var fieldValue = ctx.CurrentItem.bspBonusPerParticipant.replace(/&quot;/g, '"');
        return fieldValue;
    };

    field.DisplayForm = function (ctx) {
        var fieldValue = ctx.CurrentFieldValue.replace(/(<div dir="">)(.*)(<\/div>)/g, '$2').replace(/&quot;/g, '"');
        return fieldValue;
    };

    field.NewEdit = function (ctx) {
        var formCtx = SPClientTemplates.Utility.GetFormContextForCurrentField(ctx);
        if (formCtx === null || formCtx.fieldSchema === null) {
            return '';
        } else {
            registerCallBacks(formCtx);

            bonusPerParticipantValue = ctx.CurrentFieldValue;

            var returnValue = '<div dir="none"><input id="' + bonusPerParticipantFieldId + '" class="ms-input" type="text" style="ime-mode :inactive" size="11" disabled="true" value="' + bonusPerParticipantValue + '"/><button onclick="window.BS.Fields.BPP.calculateBonus();return false;">Calculate</button></div>';

            return returnValue;
        }
    };

    var preRenderCalled = false;
    var onPreRender = function (ctx) {
        if (!preRenderCalled) {

            preRenderCalled = true;
        }
    };

    var rederedSuccesFully = false;
    var onPostRender = function (ctx) {
        if (!rederedSuccesFully) {

            SP.SOD.executeFunc("sp.js", "SP.ClientContext", function () {

                if (!context) {
                    context = SP.ClientContext.get_current();
                }

                var web = context.get_web();
                context.load(web);

                var rs = web.get_regionalSettings();
                context.load(rs);

                context.executeQueryAsync(function () {
                    thousandsSeperator = rs.get_thousandSeparator();
                    decimalSeperator = rs.get_decimalSeparator();
                }, function () {
                    alert("Error occurred!");
                });
            });


            rederedSuccesFully = true;
        }
    };

    var bspBonusPerParticipantOverride = {
        OnPostRender: onPostRender,
        OnPreRender: onPreRender,
        Templates: {
            Fields: {
                'bspBonusPerParticipant': {
                    'View': field.View,
                    'DisplayForm': field.DisplayForm,
                    'NewForm': field.NewEdit,
                    'EditForm': field.NewEdit
                }
            }
        }
    };

    SPClientTemplates.TemplateManager.RegisterTemplateOverrides(bspBonusPerParticipantOverride);

    var registerCallBacks = function (formCtx) {
        formCtx.registerGetValueCallback(formCtx.fieldName, function () {
            return bonusPerParticipantValue;
        });
    };

})(window, window.jQuery);