window.chat = (function(){
  let connection;
  let dotnetObj;
  async function ensure(campaignId){
    if(!connection){
      if(typeof signalR === "undefined"){
        console.error("SignalR script não carregado.");
        return;
      }
      connection = new signalR.HubConnectionBuilder().withUrl('/hubs/campaign-chat').build();
      connection.on('ReceiveMessage', dto => dotnetObj && dotnetObj.invokeMethodAsync('OnReceiveMessage', dto));
      connection.on('SystemNotice', text => dotnetObj && dotnetObj.invokeMethodAsync('OnSystemNotice', text));
      await connection.start();
    }
    await connection.invoke('JoinCampaignGroup', campaignId);
  }
  return {
    async join(campaignId, objRef){ dotnetObj = objRef; await ensure(campaignId); },
    async send(campaignId, displayName, content, sentAsCharacter){
      await ensure(campaignId);
      await connection.invoke('SendMessage', campaignId, displayName, content, sentAsCharacter);
    }
  }
})();
