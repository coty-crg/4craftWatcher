
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
        var description = "<a href=\"" + threadurl + "\">/" + thread.board+ "/ - <span class=\"nameBlock\"><span class=\"name\">" + thread.name + "</span></span> " + thread.now + "</a>";
        var imghtml = "<div class=\"file\"><a class=\"fileThumb\" href=\"" + threadurl + "\" target=\"_blank\"><img src=\"https://t.4cdn.org/" + thread.board +"/"+ thread.tim + "s.jpg\" style=\"height: 125px; width: 125px;\" title /></a></div>"; 
        
        var html = "<div class=\"postContainer replyContainer\"><div class=\"post reply\"><span class=\"datetime\">" + description + "</span><br/>" + imghtml + "<blockquote class=\"postMessage\">" + thread.com + "</blockquote> </div></div>";
        $(".threadlist").append( "<div class=\"threaditem\">" + html + "</div>");
    }); 
}

function Refresh(){
    getData("https://archive.4craft.us/data", (data) => {
        var foundThreads = []; 
        var boards = data.boards;
        var searchTerm = "anon"; 
        
        console.log(data); 
        UpdateThreads(data); 
    }); 
}

Refresh(); 
setInterval(() => Refresh(), 1000 * 10); 