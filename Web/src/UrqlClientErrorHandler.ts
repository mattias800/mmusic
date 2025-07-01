import { Client, Operation } from "urql";

export const handleUrqlError = (
  operation: Operation<never>,
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  _clientProvider: () => Client,
) => {
  if (operation.kind === "mutation") {
    // triggerAppVersionCheckForModal(clientProvider(), store.dispatch);
  }
};
