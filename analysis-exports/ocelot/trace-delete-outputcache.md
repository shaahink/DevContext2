TRACE  DELETE /outputcache/{region}
       src/Administration/OutputCacheController.cs:20

▸ ENTRY  DELETE /outputcache/{region}  (src/Administration/OutputCacheController.cs:20)
   └─ call OutputCacheController.Delete  (src/Administration/OutputCacheController.cs:20)
      └─ call DefaultMemoryCache.ClearRegion  (src/Administration/OutputCacheController.cs:24) [verified]
RESULT   200 OK / 204 No Content · failure → 404 Not Found
