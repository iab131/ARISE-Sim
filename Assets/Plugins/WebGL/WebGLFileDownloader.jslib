mergeInto(LibraryManager.library, {
  DownloadFile: function(filenamePtr, contentPtr) {
    const filename = UTF8ToString(filenamePtr);
    const content = UTF8ToString(contentPtr);

    const blob = new Blob([content], { type: "application/octet-stream" });
    const a = document.createElement("a");
    a.href = URL.createObjectURL(blob);
    a.download = filename;
    a.click();
    URL.revokeObjectURL(a.href);
  }
});