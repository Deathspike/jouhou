<h1>Jouhou</h1>

Jouhou is a high-performance asynchronous object mapper for .NET 4.5/Mono 2.12 and higher. The library introduces light-weight extension methods around *DbCommand*, *DbConnection*, *DbDataReader* and *DbTransaction* class instances. It also introduces optional classes impementing a connection pool and object mapping for easier persistence.

<h2>Installation</h2>

    Install-Package Jouhou
    
<h2>Queries</h2>

Queries can be executed with the *DbConnection* or *DbTransaction* extension methods *ToAffected*, *ToScalar*, *ToResult* and *ToResultSet*. Each method is an asynchronous method written using the .NET 4.5 featured async/await. These methods either provide a dynamic or POCO class instance.

<h3>Dynamic Queries</h3>
    
    // Get a result for the first person row.
    dynamic Result = await Connection.ToResult("SELECT * FROM People LIMIT 1");

    // Get a result set for each person row where the Id is greater than 10 and the Age less than 22.
    IList<dynamic> ResultSet = await Connection.ToResultSet("SELECT * FROM People WHERE Id > @0 AND Age < @1", 10, 22);

    // Get the number of rows in the Person table.
    object NumberOfRows = await Connection.ToScalar("SELECT COUNT(*) FROM People");
    
<h3>Mapped Queries</h3>

    // Get a result for the first person row.
    Person Result = await Connection.ToResult<Person>("SELECT * FROM People LIMIT 1");
    
    // Get a result set for each person row where the Id is greater than 10 and the Age less than 22.
    IList<Person> ResultSet = await Connection.ToResultSet<Person>("SELECT * FROM People WHERE Id > @0 AND Age < @1", 10, 22);
    
    // Get the number of rows in the Person table.
    UInt64 NumberOfRows = await Connection.ToScalar<UInt64>("SELECT COUNT(*) FROM People");

<h3>Standard Queries</h3>

    // Get the number of affected rows for a delete query.
    int NumberOfRowsAffected = await Connection.ToAffected("DELETE FROM People WHERE Name = @0", "Deathspike");

<h2>Mapping</h2>

A mapping is a class which allows queries to be executed with a limited knowledge about your database scheme. It allows you to configure a table name, a primary key (which is used in *DELETE*, *INSERT* and *UPDATE* queries) and whether your primary key is an identity. The mapping allows you to perform queries while omitting the *SELECT* statement, as well as allowing persisting POCO objects using the *Save* method and deleting them using the *Delete* method.

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

    // A SELECT statement is now optional. The mapping is aware and does this automatically when missing.
    IList<Person> People = await PersonMap.ToResultSet("WHERE Id > @0 AND Age < @1", 10, 20);
    
<h3>Object Persistence</h3>

    // Create a person object.
    Person Person = new Person { Name = "Roel van Uden" };
    
    // Save the person. This will use INSERT.
    await PersonMap.Save(Person);
    
    // The person object now has the database provided identity.
    UInt64 Id = Person.Id;

Calling *Save* containg an object with a valid primary key ensures persistance uses an *UPDATE* statement.

    // Create a person object with a valid primary key.
    Person Person = new Person { Id = 1, Name = "Deathspike" };
    
    // Save the person. This will use UPDATE.
    await PersonMap.Save(Person);

It is also possible to provide multiple objects while mixing INSERT and UPDATE persistence.
    
    // Initialize an enumerable with people ...
    IEnumerable<Person> People = new List<Person> {
        // ... Use an UPDATE for this object ...
        new Person { Id = 1, Name = "Deathspike" },
        //... Use an INSERT for this object ...
        new Person { Name = "Deathspike" }
    };
    
    // Save the people. A transaction is used due to providing multiple person object instances.
    await PersonMap.Save(People);

<h3>Object Deletion</h3>

    // Find a person. It is possible to instantiate a Person class here.
    Person Person = await PersonMap.ToResult("WHERE Name = @0", "Deathspike");
    
    // Delete the person.
    await PersonMap.Delete(Person);
      
<h2>Connection Pool</h2>

A connection pool is an optional class that is available to be instantiated. The connection pool maintains a cache of database connections so that a connection can be reused in the future, thus avoiding the additional overhead of establishing a new connection. The connection pool requires a connection string and provider.

    // Create a connection pool. This is how to establish a connection using a MySQL provider.
    var ConnectionPool = new DbConnectionPool("Data Source=localhost;Port=3306;Database=Jouhou;User ID=root;Password=root;", "MySql.Data.MySqlClient");
    
    // Fetch a connection.
    var Connection = await ConnectionPool.Fetch();
      
    // Once done, recycle the connection instead of disposing of it.
    ConnectionPool.Recycle(Connection);

<h2>Afterword</h2>

Jouhou was written by Roel "Deathspike" van Uden. Suggestions/Issues/Comments? Send me a message!