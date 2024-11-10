// Only Front end

function validateCheck() {
    var SendButton = document.getElementById('SendButton');
    if ($('#cbx7').is(":checked")) {
        SendButton.classList.remove("disabledButton");
        SendButton.disabled = false;
    }
    else {
        SendButton.classList.add("disabledButton");
        SendButton.disabled = true;
    }
}

$('input[type="range"]').change(function () {
    var val = ($(this).val() - $(this).attr('min')) / ($(this).attr('max') - $(this).attr('min'));

    $(this).css('background-image',
        '-webkit-gradient(linear, left top, right top, '
        + 'color-stop(' + val + ', #f47820), '
        + 'color-stop(' + val + ', #C5C5C5)'
        + ')'
    );
});

document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        e.preventDefault();

        document.querySelector(this.getAttribute('href')).scrollIntoView({
            behavior: 'smooth'
        });
    });
});