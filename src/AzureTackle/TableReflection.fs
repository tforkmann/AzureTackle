namespace AzureTackle

module TableReflection =

    open Microsoft.WindowsAzure.Storage.Table

    let typeCache = System.Collections.Generic.Dictionary()

    let addToCache<'T> () =
        let theType = typeof<'T>
        let ctor = theType.GetConstructors().[0]
        let parameters = ctor.GetParameters()
        typeCache.Add
            (theType,
             {| Ctor = ctor
                Parameters = parameters |})

    let getOrAddFromCache<'T> () =
        match typeCache.TryGetValue typeof<'T> with
        | true, cache -> cache
        | false, _ ->
            addToCache<'T> ()
            typeCache.[typeof<'T>]

    let buildRecordFromEntity<'T> (entity: DynamicTableEntity) =
        let cache = getOrAddFromCache<'T> ()

        let args =
            [| for p in cache.Parameters do
                entity.Properties.[p.Name].PropertyAsObject |]

        cache.Ctor.Invoke(args) :?> 'T

    let buildRecordFromEntityNoCache<'T> (entity: DynamicTableEntity) =
        let theType = typeof<'T>
        let ctor = theType.GetConstructors().[0]
        let parameters = ctor.GetParameters()

        let args =
            [| for p in parameters do
                entity.Properties.[p.Name].PropertyAsObject |]

        ctor.Invoke(args) :?> 'T
