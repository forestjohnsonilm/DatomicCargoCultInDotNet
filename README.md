# Datomic Cargo Cult In Dot Net

"Cargo Cult" refers to an isolated island population immitating the appearance of advanced foriegn technology. 

[https://github.com/forestjohnsonilm/DatomicCargoCultInDotNet/blob/master/cargo-cult.jpg?raw=true]

This project is a work in progress, not functional, and purely my own academic persuit. It is not in any way associated with Datomic and does not contain any code or material from the Datomic project.

[Datomic](http://www.datomic.com/) is a new kind of database based on accretion of facts, rather than association of value(s) with id(s) like a relational or document database. The facts are formatted in such a way that they each stand alone as the smallest possible complete unit of data. Each one is called a Datom, or data atom. Each one contains: 

* Identity        (Which thing are we talking about? For example, Person1)
* Property        (What property are we talking about? For example, Email Address)
* Value           (What's the value? For example, johndoe@gmail.com)
* Retraction Flag (Is this an assertion or a retraction of this fact?)
* Transaction     (Which transaction was this assertion/retraction a part of?)

There are indexes on Identity, Property, Value, and Transaction.  Each index is useful for a different kind of query, and since it is write-only, the indexes are completely cache-able with zero cache coherence issues. The indexes fit well with LSM storage engines like Google's BigTable. 

Also, it allows the clients to run thier own queries and frees the database server to only handle transactions. Clients can subscribe to a feed of the transaction log (or a subset they care about) and maintain an index which allows them to request index segments, cache them, and run thier own queries at thier own pace. 

Datomic is called an eventually avaliable database, because its accretive nature always allows a consistent view of a point in time which can be used as an immutable value, and writing to it will not impede readers. A reader can specify the point in time they want to read from.

This is a pet project I was working on -- an attempt to replicate some of the cool functionality that Datomic provides and also learn about .NET Core. at the same time.

I was also going to use it as the foundation for the next version of my personal budget web app. 



