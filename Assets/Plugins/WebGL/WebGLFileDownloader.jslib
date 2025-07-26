mergeInto(LibraryManager.library, {
  DownloadFile: function (fileNamePtr, contentPtr) {
    const fileName = UTF8ToString(fileNamePtr);
    const content = UTF8ToString(contentPtr);

    const blob = new Blob([content], { type: "application/json" });
    const url = URL.createObjectURL(blob);

    const a = document.createElement("a");
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);

    URL.revokeObjectURL(url);
  }
});
