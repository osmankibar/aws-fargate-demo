const http = require('http');

var host = process.env.SERVER_HOST;
var id = process.env.AGENT_ID;

setTimeout(() => {
    SendRequest(`http://${host}/agents/${id}?status=Running`,0);
}, 1000);

setTimeout(() => {
    SendRequest(`http://${host}/agents/${id}?status=Finished`,0)
}, 15 * 1000);

function SendRequest(url,retryCount) {

    console.log(`url ${url}`);

    http.get(url,function(res){
        if(res.statusCode>299)
        {
            if(retryCount<5)
                SendRequest(url,retryCount+1);
        }
    })
    .on("error", (err) => {
        console.log("Error: " + err.message);
        
        if(retryCount<5)
            SendRequest(url,retryCount+1);
    });
}

