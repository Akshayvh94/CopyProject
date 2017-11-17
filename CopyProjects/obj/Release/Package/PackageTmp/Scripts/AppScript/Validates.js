var messageList = [];
var uniqueId = $('#uid').val();
var ErrorData = [];
var statusCount = 0;

var messagesCount = 10;
var percentForMessage = Math.floor(100 / messagesCount);
var currentPercentage = 0;
$(document).ready(function () {

    $('.redC').focusin(function () {
        $('.redC').css('border-color', '');
    })
    $('#modalmail').modal('show');
    $('#close').click(function () {
        $("#msgSource").hide();
        return;
    });
    $('#closeStatus').click(function () {
        $("#statusmessages").hide();
        return;
    });
    $('#closeErrList').click(function () {
        $("#errrListDiv").hide();
        return;
    });

    $("#submitForm").click(function () {
        $('#statusmessages').empty();
        $("#msg").empty();
        $("#msgSource").hide();

        messageList = [];
        var SourceAcc = $("#ddlAcccountName").val();
        var project = $("#projectSelect").val();
        var projectName = $("#projectSelect option:selected").text();
        var newprojectname = $('#NewProjectName').val();
        var TargetAcc = $("#TargetAcccountName").val();
        if (newprojectname == "") {
            $("#msg").text("Please provide a new project name");
            $("#msgSource").show();
            return;
        }
        else if (SourceAcc == '' || SourceAcc == '--select account--') {
            $("#msg").text("Please select Source Account");
            $("#msgSource").show();
            return;
        }
        else if (project == '' || project == '--select account--' || project == 0) {
            $("#msg").text("Please select Source Project");
            $("#msgSource").show();
            return;
        }
        else if (TargetAcc == '' || TargetAcc == '--select account--') {
            $("#msg").text("Please select Target Account");
            $("#msgSource").show();
            return;
        }

        else {
            $('#loader').show();
            $('#selectedAcc').val(project);
            $('#srcProjectName').val(projectName);
            // $("#Dashboardform").submit();

            var token = $('#hidaccessToken').val();
            var srcProjectID = $('#selectedAcc').val();
            var srcProjectname = $('#srcProjectName').val();
            var refToken = $('#hidrefreshToken').val();
            var targetAccount = $('#TargetAcccountName').val();
            var newprojectName = $('#NewProjectName').val();
            var useremail = $('#useremail').val();
            var projectParam = {
                accessToken: token,
                SelectedID: srcProjectID,
                accountName: SourceAcc,
                SrcProjectName: srcProjectname,
                refreshToken: refToken,
                TargetAccountName: targetAccount,
                NewProjectName: newprojectName,
                uid: uniqueId,
                Email: useremail
            }
            var WITCount = 0;

            $.post("../ProjectSetup/GetTotalWorkItemCount", projectParam, function (data) {
                if (data) {
                    if (data == '101') {
                        $('#sendmailbtn').prop('disabled', false);
                        $("#msg").html("Team project might contain more than 100 work items or more than 1 repository. Please contact" + ' <a href="mailto:vststoolssupport@ecanarys.com"> vststoolssupport@ecanarys.com</a>');
                        $('#modalmail').modal('show');
                        $("#msgSource").show();
                        $('#loader').hide();
                        return;
                    }
                    else if (data == -1) {
                        $("#msg").empty().append("Some error occured");
                        $("#msgSource").show();
                        $('#loader').hide();
                        return;
                    }
                    else {
                        $('#loader').hide();
                        $.post("../ProjectSetup/StartEnvironmentSetupProcess", projectParam, function (data) {
                            if (data != "True") {
                                window.location.href = "~/Account/Verify";
                                return;
                            }
                        });
                        $("#ddlAcccountName").prop('disabled', true);
                        $('#projectSelect').prop('disabled', true);
                        $('#TargetAcccountName').prop('disabled', true);
                        $('#NewProjectName').prop('disabled', true);
                        $("#submitForm").prop('disabled', true);
                        $('#dvProgress').show();
                        getStatus();
                    }
                }
            });

        }
    });

    $('#ddlAcccountName').change(function () {
        var accSelected = $('#ddlAcccountName').val();
        if (accSelected == '' || accSelected == '--select account--') {
            $("#msg").text("Please select a Account");
            $("#msgSource").show();
            return;
        }
        else {
            $('#projectloader').css('visibility', 'visible');

            var token = $('#hidaccessToken').val();
            var param = {
                accname: accSelected,
                pat: token
            }
            $.ajax({
                url: '../Home/GetprojectList',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(param),
                success: function (da) {
                    if (da.count > 0) {
                        $('#projectSelect').empty();
                        var opt = "";
                        // var opt = "<option value='0'>Select Project</option>";
                        for (var i = 0; i < da.count; i++) {
                            opt += ' <option value="' + da.value[i].id + '">' + da.value[i].name + '</option>';
                        }
                        $("#projectSelect").append(opt);

                        var options = $("#projectSelect option");                    // Collect options         
                        options.detach().sort(function (a, b) {               // Detach from select, then Sort
                            var at = $(a).text();
                            var bt = $(b).text();
                            return (at > bt) ? 1 : ((at < bt) ? -1 : 0);            // Tell the sort function how to order
                        });
                        options.appendTo("#projectSelect", "Select Project");
                        options.appendTo("#projectSelect");
                        $('#projectloader').css('visibility', 'hidden');
                    }
                    else {
                        $("#msg").text("Some error occured :" + da.errmsg);
                        $("#msgSource").show();
                        $('#projectloader').css('visibility', 'hidden');
                        return;
                    }

                }
            });
        }
    });

    $('#sendmailbtn').click(function () {
        var emailid = $('#emailid').val();
        var custname = $('#Custname').val();
        var checkValid = true;
        if (custname == "") {
            $('#Custname').css('border-color', 'red');
            checkValid = false;
            return;
        }
        else {
            $('#Custname').css('border-color', '');
        }
        if (emailid != "") {
            $('#emailerror').empty();
            if ($('#emailid').val().length > 0) {
                var email = $('#emailid').val();
                var filter = /^([a-zA-Z0-9_\.\-])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$/;

                if (!filter.test(email)) {
                    $('#emailerror').empty();
                    $('#emailerror').append('Please provide a valid email address');
                    $('#emailid').focus;
                    $('#emailid').css('border-color', 'red');
                    checkValid = false;
                    return;
                }
            }
        }
        else if (emailid == "") {
            $('#emailid').css('border-color', 'red');
            checkValid = false;
            return;
        }
        else {
            $('#emailid').css('border-color', '');
        }
        if (checkValid == true) {
            $('#sendmailbtn').prop('disabled', false);
            $('#mailloader').css('visibility', 'visible');
            $.ajax({
                url: '../ProjectSetup/SendAdminMail',
                type: 'POST',
                data: { mailid: emailid, name: custname },
                success: function (data) {
                    if (data == 'success') {
                        $('#mailloader').hide();
                        myFunction();
                    }
                    else {
                        $('#mailloader').hide();
                    }
                }
            });
        }

    });
    $("#emailid").on('focusout', function () {
        $('#emailerror').empty();
        if ($('#emailid').val().length > 0) {
            var email = $('#emailid').val();
            var filter = /^([a-zA-Z0-9_\.\-])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$/;

            if (!filter.test(email)) {
                $('#emailerror').empty();
                $('#emailerror').append('Please provide a valid email address');
                email.focus;
                return false;
            }
        }
    });

    $('.siteVisitor').click(function () {
        var displayname = $('#displayname').val();
        var displayEmail = $('#useremail').val();
        var email = {
            DisplayName: displayname,
            DisplayEmail: displayEmail
        }
        $.post("../ProjectSetup/SendEmailNote", email, function (data) {

        });
    });

});

function myFunction() {
    var x = document.getElementById("snackbar")
    x.className = "show";
    setTimeout(function () { x.className = x.className.replace("show", ""); }, 3000);
}

function emailvalidation() {
    $('#emailerror').empty();
    if ($('#emailid').val().length > 0) {
        var email = $('#emailid').val();
        var filter = /^([a-zA-Z0-9_\.\-])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$/;

        if (!filter.test(email)) {
            $('#emailerror').empty();
            $('#emailerror').append('Please provide a valid email address');
            email.focus;
            return false;
        }
        return true;
    }
}
function getStatus() {

    $.ajax({
        url: "../ProjectSetup/GetCurrentProgress",
        type: 'GET',
        data: { id: uniqueId },
        success: function (data) {
            var isMessageShown = true;
            $('#dvProgress').css('display', 'inline');
            $('#statusmessages').css('display', 'inline');
            if (jQuery.inArray(data, messageList) == -1) {
                messageList.push(data);
                isMessageShown = false;
            }
            if (data != "end" && data != "100") {
                if (isMessageShown == false) {
                    if (messageList.length == 1) {

                        $('#progressBar').width(currentPercentage++ + '%');
                        if (data.length > 0) {
                            $('#statusmessages').append('<img src="../Images/check-10.png" style="padding:4px;"/> ' + data + ' <br />');
                        }
                    }
                    else {
                        if (data.indexOf("TF200019") == -1) {
                            $('#progressBar').width(currentPercentage++ + '%');

                        }
                        $('#statusmessages').append('<img src="../Images/check-10.png" style="padding:4px;"/>' + data + ' <br />');

                    }

                }
                else if (currentPercentage <= ((messageList.length + 1) * percentForMessage) && currentPercentage <= 100) {
                    $('#progressBar').width(currentPercentage++ + '%');
                }
                window.setTimeout("getStatus()", 500);
            }

            else {
                stopProcess();
                var erruniqueId = uniqueId + "_Errors";
                GetErrormsg();

                $('#statusmessages').append('<a href="https://' + $('#TargetAcccountName').val() + '.visualstudio.com/' + $('#NewProjectName').val() + '" target="_blank"> Here is your new project</a><br />');
            }

        }
    });

}

function GetErrormsg() {
    var erruniqueId = uniqueId + "_Errors";
    console.log(erruniqueId);
    $.ajax({
        url: "../ProjectSetup/GetCurrentProgress",
        type: 'POST',
        data: { id: erruniqueId },
        success: function (dat) {
            if (dat != "") {
                $('#errrListDiv').css('display', 'inline');
                $('#errrListDiv').append('<br/> <img src="../Images/error.png" style="padding:4px; width:15px; height:15px;"/>' + dat + ' <br />');
            }
        }
    });
}

function stopProcess() {
    $('#dvProgress').css('display', 'none');
    $('#dvProgress').hide();
    $("#ddlAcccountName").prop('disabled', false);
    $('#projectSelect').prop('disabled', false);
    $('#TargetAcccountName').prop('disabled', false);
    $('#NewProjectName').prop('disabled', false);
    $("#submitForm").prop('disabled', false);
    currentPercentage = 0;
    messageList = [];
}