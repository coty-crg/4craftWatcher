
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

function UpdateDiscordAttachments(attachments){
	$(".discorditem").remove(); 
    
    if(attachments.length == 0){
        $(".loadingdegeneracy").replaceWith("<p class=\"nodegeneracyarchive\">No degeneracy found.</p>");
        return; 
    }
    
    $(".nodegeneracyarchive").remove(); 
    
    attachments.forEach(attachment => {
        $(".loadingdegeneracy").remove();
        
		var newWidth = attachment.width; 
		var newHeight = attachment.height; 
		
		// fudge 4chan style thumbnails  
		if(newWidth > 250 || newHeight > 250){
			if(attachment.height > attachment.width){
				var scale =  attachment.width / attachment.height; 
				newWidth = 250 * scale; 
				newHeight = 250; 
			}else{
				var scale =  attachment.height / attachment.width; 
				newWidth = 250 ; 
				newHeight = 250* scale; 
			}
		}
		
		// discord doesnt have thumbnails 
		// :V 
        var threadurl = attachment.url;  
        var description = "<a href=\"" + threadurl + "\" target=\"blank\">[" + attachment.authorUsername + 
		"]: <span class=\"nameBlock\"><span class=\"name\">" + attachment.filename + "</span></span></a>";
        var imghtml = "<div class=\"file\"><a class=\"fileThumb\" href=\"" + threadurl + "\" target=\"_blank\"><img src=" + threadurl + " style=\"height: " + newHeight + "px; width: " + newWidth + "px;\" title /></a></div>"; 
        var html = "<div class=\"postContainer replyContainer\"><div class=\"post reply\"><span class=\"datetime\">" + description + "</span><br/>" + imghtml + "<blockquote class=\"postMessage\">" + attachment.content + "</blockquote> </div></div>";
		
        $(".discordlist").append( "<div class=\"archiveitem\">" + html + "</div>");
    }); 
}

function SendData(message){
    $(".successMessage").replaceWith("<b class=\"successMessage\">Submitting..</b>"); 

    $.ajax({
        data: message,
        type: 'POST',
        processData: false,
        url: 'https://archive.4craft.us/data/submit',
        contentType: 'application/json',
        success: function (data) {
            document.getElementsByName('villagercomment')[0].value = ''; 
            $(".successMessage").replaceWith("<b class=\"successMessage\">Successfully added!</b>"); 
        }
    });

}

function Refresh(){
    getData("https://archive.4craft.us/data/live", (data) => {
        UpdateThreads(data); 
    }); 
	
	getData("https://archive.4craft.us/data/archive", (data) => {
		UpdateArchivedThreads(data); 
	});
	
	getData("https://archive.4craft.us/data/discordattachments", (data) => {
		UpdateDiscordAttachments(data);
	}); 
}

Refresh(); 
setInterval(() => Refresh(), 1000 * 60); 