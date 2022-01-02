namespace LightADO;

/// <summary>
/// This Event will be fired after closing the connection.
/// </summary>
public delegate void AfterCloseConnection();


/// <summary>
/// This Event will be fired after Execute the command.
/// </summary>
public delegate void AfterExecute();


/// <summary>
/// This Event will be fired before Execute the command.
/// </summary>
public delegate void BeforeExecute();


/// <summary>
/// This Event will be fired after closing the connection.
/// </summary>
public delegate void BeforeCloseConnection();


/// <summary>
/// This Event will be fired before open the connection.
/// </summary>
public delegate void BeforeOpenConnection();

/// <summary>
/// This Event will be fired before open the connection.
/// </summary>
public delegate void AfterOpenConnection();

/// <summary>
/// This Event will be fired when there's an error
/// </summary>
public delegate void OnError(LightAdoExcption ex);