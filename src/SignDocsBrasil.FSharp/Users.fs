namespace SignDocsBrasil.FSharp

open SignDocsBrasil.Api
open SignDocsBrasil.Api.Models

module Users =
    /// Enrolls a user with the given external ID.
    let enroll (userExternalId: string) (request: EnrollUserRequest) (client: SignDocsBrasilClient) =
        tryCallAsync (fun () -> client.Users.EnrollAsync(userExternalId, request))
