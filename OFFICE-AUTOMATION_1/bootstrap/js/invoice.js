

 function noconvertwordfile()
    {
        $("#nocovert").addClass("hidden");
        $("#wordfile").addClass("hidden");
    }

window.onload = function () {
    var ttlsum = 0;
    for (var j = 1; j < id; j++) {

        var amount = parseInt(document.getElementById('amount_' + j).innerHTML);
        ttlsum = ttlsum + amount;
       
        if (amount == null || amount == "") {
            amount = 0;
        }
    }
    document.getElementById('ttlsum').innerHTML = ttlsum;
    var word = inWords(ttlsum);
    document.getElementById('ttlsumword').innerHTML = word;
}

function HtmlToWord() {
    var markup = document.documentElement.innerHTML;
    $("#htmltxt").val(markup);
    var filename = $("#filename").val().concat(".docx");
    // alert(filename);
    $.ajax({
        type: "post",
        url: "/CreateFileFromHTML",
        data: { html: markup, fileName: filename },

        success: function (data) {
        }, error: function (xhr, textStatus, errorThrown) {
            alert(xhr.responseText);
        }
    })
}