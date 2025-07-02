const apiBasePath = "http://localhost:5095/graphql";

export const getApiUrl = () => {
  return apiBasePath;
};

export const getWsApiUrl = () => {
  const loc = window.location;
  let wsUri: string = "";
  if (loc.protocol === "https:") {
    wsUri += "wss:";
  } else {
    wsUri += "ws:";
  }
  wsUri += "//" + loc.host;
  wsUri += apiBasePath;

  return wsUri;
};
