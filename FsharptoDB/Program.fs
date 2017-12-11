// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
open System
open System.Data
open System.Data.Linq
open FSharp.Data.TypeProviders
open FSharp.Linq


// dbSchema, which is a parent type that contains all the generated types that represent database tables. 
type dbSchema = SqlDataConnection<"Data Source=APPARAT\SQLDEVELOPER;Initial Catalog=MyDatabase;Integrated Security=SSPI;">
// The object, db, which has as its members all the tables in the database.
let db = dbSchema.GetDataContext()
let table1 = db.Table1
let query1 = 
    query {
      for row in db.Table1 do
      select row
     }
query1 |> Seq.iter (fun row -> printfn "%s %d" row.Name row.TestData1)

// This example shows that you can eliminate the query variable and use a pipeline operator instead.
query {
   for row in db.Table1 do
   where (row.TestData1 > 2)
   select row
 } |> Seq.iter (fun row -> printfn "%d %s" row.TestData1 row.Name)

// a more complex query with a join of two tables.
query {
   for row1 in db.Table1 do
   join row2 in db.Table2 on (row1.Id = row2.Id)
   select (row1, row2)
 } |> Seq.iteri (fun index (row1, row2) ->
                 if (index = 0) then printfn "Table1.Id TestData1 TestData2 Name Table2.Id TestData1 TestData2 Name"
                 printfn "%d %d %f %s %d %d %f %s" row1.Id row1.TestData1 row1.TestData2 row1.Name
                   row2.Id (row2.TestData1.GetValueOrDefault()) (row2.TestData2.GetValueOrDefault()) row2.Name)

// the following code that wraps a query in a function that takes a parameter, and then calls that function with the value 10.
let findData param =
   query {
     for row in db.Table1 do
     where (row.TestData1 = param)
     select row
   }

findData 10 |> Seq.iter (fun row -> printfn "Found row: %d %d %f %s" row.Id row.TestData1 row.TestData2 row.Name)


//-------------------------------
// Working with nullable fields
//-------------------------------

// The following code shows working with nullable values; assume that TestData1 is an integer field that allows nulls.
query {
  for row in db.Table2 do
  where (row.TestData1.HasValue && row.TestData1.Value > 2)
  select row
} |> Seq.iter (fun row -> printfn "%d %s" row.TestData1.Value row.Name)

query {
  for row in db.Table2 do
  // Use a nullable operator ?>
  where (row.TestData1 ?> 2)
  select row
} |> Seq.iter (fun row -> printfn "%d %s" (row.TestData1.GetValueOrDefault()) row.Name)


//------------------------------
// Calling a stored procedure
//------------------------------

// The following code assumes that there is a procedure Procedure1 on the database that takes two nullable integers as parameters,
// runs a query that returns a column named TestData1, and returns an integer.
type schema = SqlDataConnection<"Data Source=APPARAT\SQLDEVELOPER;Initial Catalog=MyDatabase;Integrated Security=SSPI;", StoredProcedures = true>

let testdb = schema.GetDataContext()

let nullable value = new System.Nullable<_>(value)

let callProcedure1 a b =
  let results = testdb.Procedure1(nullable a, nullable b)
  for result in results do
    printfn "%d" (result.TestData1.GetValueOrDefault())
  results.ReturnValue :?> int

printfn "Return Value: %d" (callProcedure1 10 20)


//-----------------------------
// Updating the database
//-----------------------------



// Enable the logging of database activity to the console.
db.DataContext.Log <- System.Console.Out


