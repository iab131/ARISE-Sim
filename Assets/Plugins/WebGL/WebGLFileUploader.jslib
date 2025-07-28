mergeInto(LibraryManager.library, {
  UploadJsonFile: function (gameObjectNamePtr, methodNamePtr) {
    const gameObjectName = UTF8ToString(gameObjectNamePtr);
    const methodName = UTF8ToString(methodNamePtr);

    let input = document.createElement('input');
    input.type = 'file';
    input.accept = '.fllrobot';

    input.onchange = (e) => {
      const file = e.target.files[0];
      if (!file) return;

      const reader = new FileReader();
      reader.onload = function (event) {
        const encrypted = event.target.result;
        // Optionally decrypt in JS or just send to Unity
        unityInstance.SendMessage(gameObjectName, methodName, encrypted);
      };
      reader.readAsText(file);
    };

    input.click();
  }
});
