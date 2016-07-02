﻿namespace Nancy.Session.Persistable

open Nancy
open Nancy.Cryptography
open Nancy.Session
open Newtonsoft.Json
open System.Collections.Generic
open System

/// Interface for a persistable session
[<AllowNullLiteral>]
type IPersistableSession = 
  inherit ISession

  /// Get a strongly-typed value from the session
  abstract Get<'T> : string -> 'T

  /// Get a strongly-typed value from the session, or the given value if it does not exist
  abstract GetOrDefault<'T> : string * 'T -> 'T


/// A base class for persistable sessions
type BasePersistableSession(values : IDictionary<string, obj>) =
  inherit Session(values)

  new() = BasePersistableSession(Dictionary<string, obj>())

  interface IPersistableSession with
    member this.GetOrDefault<'T> (key, value) =
      match this.[key] with
      | null          -> value
      | :? 'T as item -> item
      | item          -> try JsonConvert.DeserializeObject<'T>(string item)
                          with | _ -> value

    member this.Get<'T> key = this.GetOrDefault (key, Unchecked.defaultof<'T>)

  /// <summary>
  /// Get a strongly-typed value from the session
  /// </summary>
  /// <param name="key">The key for the session value</param>
  /// <returns>The value (or a default of that type if the value is not set or does not deserialize)</returns>
  abstract Get<'T> : string -> 'T
  default this.Get<'T> key = (this :> IPersistableSession).Get<'T> key

  /// <summary>
  /// Get a strongly-typed value from the session
  /// </summary>
  /// <param name="key">The key for the session value</param>
  /// <param name="value">The default value to return if the key is not found or it does not deserialize</param>
  /// <returns>The value from the session, or the default value if it cannot be obtained from the session</returns>
  abstract GetOrDefault<'T> : string * 'T -> 'T
  default this.GetOrDefault<'T> (key, value : 'T) = (this :> IPersistableSession).GetOrDefault (key, value)

/// Base configuration options for persistable sessions
[<AllowNullLiteral>]
type BasePersistableSessionConfiguration(cryptoConfig : CryptographyConfiguration) as this =
  inherit CookieBasedSessionsConfiguration(cryptoConfig)
  
  do
    this.Serializer <- DefaultObjectSerializer()

  /// Gets or sets whether to use rolling sessions (expiry based on inactivity) or not (expiry based on creation)
  member val UseRollingSessions = true with get, set

  /// Gets or sets the session expiry period
  member val Expiry = TimeSpan(2, 0, 0) with get, set

  /// Gets or sets the frequency with which expired sessions are removed from storage
  member val ExpiryCheckFrequency = TimeSpan(0, 1, 0) with get, set

  /// Get the validity state of the session
  override this.IsValid with get() = base.IsValid
