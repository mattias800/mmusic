import { AnyVariables, Client, Operation } from "urql";

export const handleUrqlError = (
  operation: Operation<never, AnyVariables>,
  _clientProvider: () => Client,
) => {
  if (operation.kind === "mutation") {
    // triggerAppVersionCheckForModal(clientProvider(), store.dispatch);
  }
};
