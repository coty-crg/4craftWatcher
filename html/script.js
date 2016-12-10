
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
        var description = "<a href=\"" + threadurl + "\">/" + thread.board+ "/ - Name: " + thread.name + " Date: " + thread.now + "</a>";
        var imghtml = "<img src=\"https://i.4cdn.org/" + thread.board +"/"+ thread.tim + "s" + thread.ext + "\"/>"; 
        var html = "<div class=\"reply\"><span class=\"datetime\">" + description + "</span><br/>" + imghtml + "<blockquote class=\"postMessage\">" + thread.com + "</blockquote> </div>";
        $(".threadlist").append( "<li class=\"threaditem\">" + html + "</li>");
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