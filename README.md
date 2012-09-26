<h1>Jouhou</h1>

Jouhou is a high-performance asynchronous object mapper for .NET 4.5/Mono 2.12 and higher. The library introduces light-weight extension methods for *DbCommand*, *DbConnection*, *DbDataReader* and *DbTransaction* class instances. Certain optional classes have been introduced as well, implementing a connection pool and an object mapping abstraction for simple persistence.

<h2>Installation</h2>

The library can be installed using <a href="http://nuget.org/packages/Jouhou">NuGet package manager</a>. Documentation regarding the package manager can be found on <a href="http://docs.nuget.org/docs/start-here/using-the-package-manager-console">the package manager website</a>. It is also possible to clone the repository and compile the library using your favorite integrated development environment or command line tool.

    Install-Package Jouhou
    
<h2>Queries</h2>

Queries can be executed with the *DbConnection* or *DbTransaction* extension methods *AffectedAsync*, *AllAsync*, *ScalarAsync*, and *SingleAsync*. Each method is an asynchronous method written using the .NET 4.5 async/await keywords. The queries return either dynamic or typed instances. A dynamic instance is not case sensitive.

<h3>Affected Queries</h3>

    // Retrieve the number of affected rows for this delete.
    int NumberOfRowsAffected = await Connection.AffectedAsync("DELETE FROM People WHERE Name = @0", "Deathspike");

<h3>Dynamic Queries</h3>
    
    // Retrieve a person.
    dynamic Single = await Connection.SingleAsync("SELECT * FROM People LIMIT 1");

    // Retrieve each person where the Id is greater than 10 and Age is less than 22.
    var All = await Connection.AllAsync("SELECT * FROM People WHERE Id > @0 AND Age < @1", 10, 22);

    // Retrieve a scalar indicating the number of people stored in the table.
    object NumberOfRows = await Connection.ScalarAsync("SELECT COUNT(*) FROM People");
    
<h3>Typed Queries</h3>

    // Retrieve a person.
    Person Single = await Connection.SingleAsync<Person>("SELECT * FROM People LIMIT 1");
    
    // Retrieve each person where the Id is greater than 10 and Age is less than 22.
    List<Person> All = await Connection.AllAsync<Person>("SELECT * FROM People WHERE Id > @0 AND Age < @1", 10, 22);
    
    // Retrieve a scalar indicating the number of people stored in the table.
    UInt64 NumberOfRows = await Connection.ScalarAsync<UInt64>("SELECT COUNT(*) FROM People");

<h2>Pooling</h2>

The *DbConnectionPool* implements a connection pool. The connection pool maintains a cache of database connections so that a connection can be reused in the future. This will avoid the additional overhead of establishing a connection and will improve the response time. The connection pool requires a connection string and provider.

    // Retrieve the connection configuration from the configuration file.
    var Configuration = ConfigurationManager.ConnectionStrings["MyConnection"];
    
    // Instantiate a connection pool.
    var ConnectionPool = new DbConnectionPool(Configuration.ConnectionString, Configuration.ProviderName);
    
    // Demonstrate the performance benefits of pooling ...
    for (int i = 0; i < 100; i++) {
        // Fetch a connection (First: Establish connection, Later: Reuse Connection).
        var Connection = await ConnectionPool.OpenAsync();
        
        // Interact with the database.
        var Example = await Connection.AffectedAsync("DELETE * FROM People");
        
        // Close the connection. Disposing of the connection is not allowed when pooling.
        ConnectionPool.Close(Connection);
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

<h2>Mapping</h2>

The *Mapping* class implements an object mapping abstraction for simple persistence. It is configured to have limited information about your database scheme and does allow the configuration of the *table name*, *primary key* and *identity*. The *table name* is used when queries omit a *SELECT* statement. The *primary key* is used when persisting an object using the *SaveAsync* method and when deleting an object using the *DeleteAsync* method. When using the *SaveAsync* method, the library determines whether to use an *INSERT* or *UPDATE* statement and does so based on the primary key. If it is determined to use an *INSERT* statement, the *Identity* configuration ensures that the persisting object is assigned the database generated identity.

<h3>Defining a Mapping Class</h3>

    // Mapping for the Person class; use PersonMap instead of PersonMap<Person> for dynamic mapping.
    class PersonMap : Mapping<Person> {
        // Must have a connection pool; otherwise methods omitting connection/transaction will fail.
        public PersonMap(ConnectionPool ConnectionPool)
            : base(
                // The connection pool
                ConnectionPool: ConnectionPool,
                // Indicates that the primary key is an identity field.
                Identity: true,
                // The primary key. This is necessary for automated DELETE and UPDATE queries.
                PrimaryKey: "Id",
                // Set the table name. If not set, this will default to the name of the class.
                TableName: "People") {
        }
    }
    
<h3>Basic Queries</h3>

    // Omit the SELECT statement and retrieve each person where the Id is greater than 10 and Age is less than 22.
    List<Person> People = await PersonMap.AllAsync("WHERE Id > @0 AND Age < @1", 10, 20);
    
<h3>Persisting an Object</h3>

    // Instantiate a person and assign the name.
    var Person = new Person { Name = "Deathspike" };
    
    // Save the person. The primary key is invalid and will cause an INSERT statement to be used.
    await PersonMap.SaveAsync(Person);
    
    // Retrieve the identifier of the person. This has been assigned the identity the database provided.
    var Id = Person.Id;

Using an object with a valid primary key will cause an *UPDATE* statement to be used ...

    // Instantiate a person and assign the name and primary key.
    var Person = new Person { Id = 1, Name = "Deathspike" };
    
    // Save the person. The primary key is valid and will cause an UPDATE statement to be used.
    await PersonMap.SaveAsync(Person);

Using an enumerable with more than once object will result in a transaction ...
    
    // Initialize an enumerable with multiple person instantiations ...
    var People = new List<Person> {
        // ... instantiate a person causing an UPDATE statement ...
        new Person { Id = 1, Name = "Deathspike" },
        // ... and instantiate a person causing an INSERT statement ...
        new Person { Name = "Deathspike" }
    };
    
    // Save the people. A transaction will be used to save the batch of people.
    await PersonMap.SaveAsync(People);

Using an established connection or transaction can make persistence of multiple batches safer and easier ...

    // Open a connection.
    var Connection = await ConnectionPool.OpenAsync();

    // Begin a transaction.
    using (var Transaction = Connection.BeginTransaction()) {
        // Save the people
        await PersonMap.SaveAsync(Transaction, People);

        // Save the computers owned by the people too.
        await ComputerMap.SaveAsync(Transaction, Computers);

        // Commit the transaction.
        Transaction.Commit();
    }

    // Close the connection.
    ConnectionPool.Close(Connection);

<h3>Deleting an Object</h3>

    // Retrieve a person. A person object could be instantiated instead.
    var Person = await PersonMap.SingleAsync("WHERE Name = @0", "Deathspike");
    
    // Delete the person. This will cause the object to be deleted only on the database.
    await PersonMap.DeleteAsync(Person);

<h2>Afterword</h2>

Jouhou was written by Roel "Deathspike" van Uden. If you have comments, questions or suggestions I would love to hear from you! To contact me, you can send me an e-mail or talk to me on the <a href="http://chat.stackoverflow.com/rooms/7/c">StackOverflow C# chat room</a>. Thank you for your interest in the Jouhou high-performance asynchronous object mapper!