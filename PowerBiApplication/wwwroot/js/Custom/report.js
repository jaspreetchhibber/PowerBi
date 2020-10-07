function BindReport() {
    // Get models. models contains enums that can be used.
    var models = window['powerbi-client'].models;
    var config = {
        type: 'report',
        accessToken: accessToken,
        embedUrl: embedUrl,
        id: reportId,
        permissions: models.Permissions.All,
        settings: {
            filterPaneEnabled: true,
            navContentPaneEnabled: true,
        }
    };

    // Grab the reference to the div HTML element that will host the report.
    var reportContainer = document.getElementById('reportContainer');
    // Embed the report and display it within the div container.
    report = powerbi.embed(reportContainer, config);
}