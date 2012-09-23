<h1>Jouhou</h1>

Jouhou is a high-performance asynchronous object mapper for .NET 4.5/Mono 2.12 and higher. The library introduces light-weight extension methods for *DbCommand*, *DbConnection*, *DbDataReader* and *DbTransaction* class instances. Certain optional classes have been introduced as well, implementing an object mapping abstraction for simple persistence and a connection pool.

<h2>Installation</h2>

The library can be installed using <a href="http://nuget.org/packages/Jouhou">NuGet package manager</a>. Documentation regarding the package manager can be found on <a href="http://docs.nuget.org/docs/start-here/using-the-package-manager-console">the package manager website</a>. It is also possible to clone the repository and compile the library using your favorite integrated development environment or command line tool.

    Install-Package Jouhou
    
<h2>Queries</h2>

Queries can be executed with the *DbConnection* or *DbTransaction* extension methods *ToAffected*, *ToScalar*, *ToResult* and *ToResultSet*. Each method is an asynchronous method written using the .NET 4.5 async/await keywords. The queries return either dynamic instances or structured class instances (POCO).

<h3>Dynamic Queries</h3>
    
    // Retrieve a person.
    dynamic Result = await Connection.ToResult("SELECT * FROM People LIMIT 1");

    // Retrieve each person where the Id is greater than 10 and Age is less than 22.
    List<dynamic> ResultSet = await Connection.ToResultSet("SELECT * FROM People WHERE Id > @0 AND Age < @1", 10, 22);

    // Retrieve a scalar indicating the number of people in the table.
    object NumberOfRows = await Connection.ToScalar("SELECT COUNT(*) FROM People");
    
<h3>Mapped Queries</h3>

    // Retrieve a person.
    Person Result = await Connection.ToResult<Person>("SELECT * FROM People LIMIT 1");
    
    // Retrieve each person where the Id is greater than 10 and Age is less than 22.
    List<Person> ResultSet = await Connection.ToResultSet<Person>("SELECT * FROM People WHERE Id > @0 AND Age < @1", 10, 22);
    
    // Retrieve a scalar indicating the number of people in the table.
    UInt64 NumberOfRows = await Connection.ToScalar<UInt64>("SELECT COUNT(*) FROM People");

<h3>Plain Queries</h3>

    // Retrieve the number of affected rows for this delete.
    int NumberOfRowsAffected = await Connection.ToAffected("DELETE FROM People WHERE Name = @0", "Deathspike");

<h2>Mapping</h2>

The *DbMap<T>* class implements an object mapping abstraction for simple persistence. It is configured to have limited information about your database scheme and does allow the configuration of the *table name*, *primary key* and *identity*. The *table name* defaults to the name of *T* and is used when queries omit a *SELECT* statement. The *primary key* is used when persisting an object using the *Save* method and when deleting an object using the *Delete* method. When using the *Save* method, the librarry determines whether to use an *INSERT* or *UPDATE* statement and does so based on the primary key. If it is determined to use an *INSERT* statement, the *Identity* configuration ensures that the persisting object is assigned the database generated identity.

<h3>Defining a Mapping Class</h3>

    // Map for the Person class.
    class PersonMap : DbMap<Person> {
        // Constructor should pass a DbConnection or DbConnectionPool.
        public PersonMap(DbConnection Connection)
            : base(Connection) {
            // Indicates that the primary key is an identity field.
            Identity = true;
            // The primary key. This is necessary for automated DELETE, INSERT and UPDATE queries.
            PrimaryKey = "Id";
            // Set the table name. If not set, this will default to the name of the class.
            TableName = "People";
        }
    }
    
<h3>Mapped Queries</h3>

    // Omit the SELECT statement and retrieve each person where the Id is greater than 10 and Age is less than 22.
    List<Person> People = await PersonMap.ToResultSet("WHERE Id > @0 AND Age < @1", 10, 20);
    
<h3>Persisting an Object</h3>

    // Instantiate a person and assign the name.
    var Person = new Person { Name = "Deathspike" };
    
    // Save the person. The primary key is invalid and will cause an INSERT statement to be used.
    await PersonMap.Save(Person);
    
    // Retrieve the identifier of the person. This has been assigned the identity the database provided.
    var Id = Person.Id;

Using the same method using an object with a valid primary key will cause an *UPDATE* statement to be used.

    // Instantiate a person and assign the name and primary key.
    var Person = new Person { Id = 1, Name = "Deathspike" };
    
    // Save the person. The primary key is valid and will cause an UPDATE statement to be used.
    await PersonMap.Save(Person);

It is also possible to persist multiple object instances using a single class.
    
    // Initialize an enumerable with multiple person instantiations ...
    var People = new List<Person> {
        // ... instantiate a person causing an UPDATE statement ...
        new Person { Id = 1, Name = "Deathspike" },
        // ... and instantiate a person causing an INSERT statement ...
        new Person { Name = "Deathspike" }
    };
    
    // Save the people. A transaction will be used to save the batch of people.
    await PersonMap.Save(People);

<h3>Deleting an Object</h3>

    // Retrieve a person. A person object could be instantiated instead.
    var Person = await PersonMap.ToResult("WHERE Name = @0", "Deathspike");
    
    // Delete the person. This will cause the object to be deleted only on the database.
    await PersonMap.Delete(Person);
    
<h2>Pooling</h2>

The *DbConnectionPool* implements a connection pool. The connection pool maintains a cache of database connections so that a connection can be reused in the future. This will avoid the additional overhead of establishing a connection and will improve the response time. The connection pool requires a connection string and provider.

    // Retrieve the connection configuration from the configuration file.
    var Configuration = ConfigurationManager.ConnectionStrings["MyConnection"];
    
    // Instantiate a connection pool.
    var ConnectionPool = new DbConnectionPool(Configuration.ConnectionString, Configuration.ProviderName);
    
    // Establish the point about pooling ...
    for (int i = 0; i < 100; i++) {
        // Fetch a connection (First: Establish connection, Later: Reuse Connection).
        var Connection = await ConnectionPool.Fetch();
        
        // Interact with the database.
        var Example = await Connection.ToAffected("DELETE * FROM People");
        
        // Recycle the connection. Disposing of the connection is not allowed when pooling.
        ConnectionPool.Recycle(Connection);
    }

<h3>Configuration for MySQL</h3>

    <?xml version="1.0" encoding="utf-8" ?>
    <configuration>
        <connectionStrings>
    		<add name="MyConnection"
    			 connectionString="Data Source=localhost;Port=3306;Database=Jouhou;User ID=root;Password=root;"
    			 providerName="MySql.Data.MySqlClient" />
    	</connectionStrings>
    	<system.data>
    		<DbProviderFactories>
    			<add name="MySQL Data Provider"
    				 invariant="MySql.Data.MySqlClient"
    				 description=".Net Framework Data Provider for MySQL"
    				 type="MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data, Version=6.5.4.0, Culture=neutral, PublicKeyToken=c5687fc88969c44d" />
    		</DbProviderFactories>
    	</system.data>
    </configuration>

<h2>Afterword</h2>

Jouhou was written by Roel "Deathspike" van Uden. If you have comments, questions or suggestions I would love to hear from you! To contact me, you can send me an e-mail or talk to me on the <a href="http://chat.stackoverflow.com/rooms/7/c">StackOverflow C# chat room</a>. Thank you for your interest in the Jouhou high-performance asynchronous object mapper!