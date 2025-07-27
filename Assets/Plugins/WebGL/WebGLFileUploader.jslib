mergeInto(LibraryManager.library, {
  UploadJsonFile: function(gameObjectNamePtr, methodNamePtr) {
    const gameObjectName = UTF8ToString(gameObjectNamePtr);
    const methodName = UTF8ToString(methodNamePtr);

    let input = document.createElement('input');
    input.type = 'file';
    input.accept = '.json';

    input.onchange = (e) => {
      const file = e.target.files[0];
      if (!file) return;

      const reader = new FileReader();
      reader.onload = function(event) {
        const json = event.target.result;
        // Send to Unity
        unityInstance.SendMessage(gameObjectName, methodName, json);
      };
      reader.readAsText(file);
    };

    input.click();
  }
});
