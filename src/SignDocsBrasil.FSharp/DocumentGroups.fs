namespace SignDocsBrasil.FSharp

open SignDocsBrasil.Api
open SignDocsBrasil.Api.Models

module DocumentGroups =
    /// Gets the combined stamp for a document group.
    let combinedStamp (groupId: string) (client: SignDocsBrasilClient) =
        tryCallAsync (fun () -> client.DocumentGroups.CombinedStampAsync(groupId))
