var azureSearchQueryApiKey = "82C2E107EEB733CB613EBD24977BCB0E";
var inSearch = false;

function execSuggest()
{
	// Execute a search to lookup viable movies
	var q = encodeURIComponent($("#q").val());
	var searchAPI = "https://azs-playground.search.windows.net/indexes/tate-art-collection/docs?$top=12&$select=acno,title,all_artists,thumbnailUrl,url&api-version=2015-02-28&search=" + q;
	inSearch= true;
    $.ajax({
        url: searchAPI,
        beforeSend: function (request) {
            request.setRequestHeader("api-key", azureSearchQueryApiKey);
            request.setRequestHeader("Content-Type", "application/json");
            request.setRequestHeader("Accept", "application/json; odata.metadata=none");
        },
        type: "GET",
        success: function (data) {
			$( "#mediaContainer" ).html('');
			for (var item in data.value)
			{
				var id = data.value[item].acno;
				var title = data.value[item].title;
				var all_artists = data.value[item].all_artists;
				var url = data.value[item].url;
				var imageURL = data.value[item].thumbnailUrl;
				if (imageURL == null)
					imageURL = "no_image.png";
				$( "#mediaContainer" ).append( '<div class="col-md-4" style="text-align:center"><a href="' + url + '"><img src=' + imageURL + ' height=200><br><div style="height:100px"><b>' + title + '</b></a><br>' + all_artists + '</div></div>' );
			}
			inSearch= false;
        }
    });
}
