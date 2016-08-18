var test = null;

//////////////// Popup for Sites Parameters ////////////////////////////
var paramObj;
var paramArray;

$(function () {
    //add organizations from the project summary clicked to open popup dialog to add organizations to a project
    $("#ParamLink").button();

    $("#ParamDialog").dialog({
        dialogClass: 'dialog_style2',
        autoOpen: false,
        height: 'auto',
        width: 'auto',
        resizable: false,
        modal: true,
        buttons: {
            "Finished Adding Parameters": function () {
                $("#update-message").html(''); //make sure nothing there
                $("#updateParamForm").submit();
            },
            "Cancel": function () {
                $(this).dialog("close");
            }
        }
    });

    $("#ParamLink").click(function () {
        $("#ParametersUL").hide();
        //populate paramStr with $("#SiteParams").val if it's not ""
        if (paramArray != undefined && $("#SiteParams").val() == "") {
            var paramStr = new String(paramArray.toString());
        }
        else {
            var paramStr = $("#SiteParams").val();
        }
        paramObj = $(this);
        var dialogDiv = $("#ParamDialog");
        var viewUrl = paramObj.attr('href');
        if (paramArray != undefined || $("#SiteParams").val() != "") {
            $.ajax({
                url: viewUrl,
                type: 'GET',
                data: { paramS: paramStr },
                success: function (data) {
                    dialogDiv.html(data);
                    //validation
                    var $form = $("#updateParamForm");
                    //unbind existing validation
                    $form.unbind();
                    $form.data("validator", null);
                    //check document for changes
                    $.validator.unobtrusive.parse(document);
                    //re add validation with changes
                    $form.validate($form.data("unobtrusiveValidation").options);
                    //open dialog
                    dialogDiv.dialog("open");
                }
            });
        }
        else {
            $.get(viewUrl, function (data) {
                dialogDiv.html(data);
                //validation
                var $form = $("#updateParamForm");
                //unbind existing validation
                $form.unbind();
                $form.data("validator", null);
                //check document for changes
                $.validator.unobtrusive.parse(document);
                //re add validation with changes
                $form.validate($form.data("unobtrusiveValidation").options);
                //open dialog
                dialogDiv.dialog("open");
            });
        }
        return false;
    });
});

function ParamSuccess(data) {
    //dialog success from post of dialog popup containing parameters for site. puts those chosen into textarea
    if (data.Success == true) {
        paramArray = data.Object;
        var resultHTML = "";
        for (i = 0; i < paramArray.length; i++) {
            resultHTML += paramArray[i] + ", ";
        }
        $("#SiteParams").val(resultHTML);
        $("#successParam").css({ 'visibility': 'visible', display: 'inline-block' });

        //now we can close the dialog
        $('#ParamDialog').dialog('close');
        //twitter type notification
        $('#commonMessage').html("Update Complete");
        $('#commonMessage').delay(400).slideDown(400).delay(3000).slideUp(400);
    }
    else {
        $("#update-message").html(data.ErrorMessage);
        $("#update-message").show();
    }
}
