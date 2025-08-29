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
      try {
        await connection.start();
      } catch (err) {
        console.error('Falha ao conectar ao SignalR', err);
        return;
      }
    }
    try {
      await connection.invoke('JoinCampaignGroup', campaignId);
    } catch (err) {
      console.error('Falha ao entrar no grupo', err);
    }
  }
  return {
    async join(campaignId, objRef){ dotnetObj = objRef; await ensure(campaignId); },
    async send(campaignId, displayName, content, sentAsCharacter){
      await ensure(campaignId);
      await connection.invoke('SendMessage', campaignId, displayName, content, sentAsCharacter);
    }
  }
})();
