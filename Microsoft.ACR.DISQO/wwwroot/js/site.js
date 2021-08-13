// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function LinkGenerator(value, row, index) {

    return value != "" ? "<a href='" + value + "' target='_blank'> Link </a>" : "";
}

function TextGenerator(value, row, index) {

    return value !== null ? value : "";
}

function RowNumberGenerator(value, row, index) {
    return "<span title='" + value + "' >" + (index + 1) + "</span>";
}

function SpanGenerator(value, row, index) {
    return value != undefined ? "<span title='" + value + "'>" + value.substring(1, 15) + "... </span>" : "";
}

function ButtonGenerator(value, row, index) {
    return value == "" ? "" : '<input type="button" id="btnRow' + index + '" class="btn btn-outline-success btnRow" name="' + value + '" value="Pass" />';
}

function DateFormatter(value, row, index) {
    return new Date(value).toLocaleString(undefined, {
        day: 'numeric',
        month: 'numeric',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit'
    }).replace(",", "");
}