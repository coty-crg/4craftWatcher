
function getData(url, callback){
    $.ajax({
        accepts: 'application/json',
        type: 'GET',
        dataType: 'json',
        url: url,
        success: function (data) {
            callback(data);
        }
    });
}

function UpdateThreads(threads){
    $(".threaditem").remove(); 
    
    if(threads.length == 0){
        $(".loading").replaceWith("<p class=\"nothreads\">No threads found.</p>");
        return; 
    }
    
    $(".nothreads").remove(); 
    
    threads.forEach(thread => {
        $(".loading").remove();
        
        var threadurl = "https://boards.4chan.org/" + thread.board + "/thread/" + thread.no;  
        var description = "<a href=\"" + threadurl + "\"  target=\"blank\">/" + thread.board+ "/ - <span class=\"nameBlock\"><span class=\"name\">" + thread.name + "</span></span> " + thread.now + "</a>";
        var imghtml = "<div class=\"file\"><a class=\"fileThumb\" href=\"" + threadurl + "\" target=\"_blank\"><img src=\"https://t.4cdn.org/" + thread.board +"/"+ thread.tim + "s.jpg\" style=\"height: 125px; width: 125px;\" title /></a></div>"; 
        
        var html = "<div class=\"postContainer replyContainer\"><div class=\"post reply\"><span class=\"datetime\">" + description + "</span><br/>" + imghtml + "<blockquote class=\"postMessage\">" + thread.com + "</blockquote> </div></div>";
        $(".threadlist").append( "<div class=\"threaditem\">" + html + "</div>");
    }); 
}

function UpdateArchivedThreads(threads){
	    $(".archiveitem").remove(); 
    
    if(threads.length == 0){
        $(".loadingarchive").replaceWith("<p class=\"nothreadsarchive\">No archived threads found.</p>");
        return; 
    }
    
    $(".nothreadsarchive").remove(); 
    
    threads.forEach(thread => {
        $(".loadingarchive").remove();
        
        var threadurl = "http://archived.moe/" + thread.board + "/thread/" + thread.no;  
        var description = "<a href=\"" + threadurl + "\" target=\"blank\">/" + thread.board+ "/ - <span class=\"nameBlock\"><span class=\"name\">" + thread.name + "</span></span> "
         + thread.now + "</a> [unique IPs: " + thread.unique_ips + ", replies: " + thread.replies + ", images: " + thread.images + "]";
        var html = "<div class=\"postContainer replyContainer\"><div class=\"post reply\"><span class=\"datetime\">" + description + "</span><br/></div></div>";
        $(".archivelist").append( "<div class=\"archiveitem\">" + html + "</div>");
    }); 
}

function Refresh(){
    getData("https://archive.4craft.us/data/live", (data) => {
        UpdateThreads(data); 
    }); 
	
	getData("https://archive.4craft.us/data/archive", (data) => {
		UpdateArchivedThreads(data); 
	});
}

Refresh(); 
setInterval(() => Refresh(), 1000 * 10); 