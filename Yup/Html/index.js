function debounce(func, wait, immediate) {
  var timeout;
  return function () {
    var context = this, args = arguments;
    var later = function () {
      timeout = null;
      if (!immediate) func.apply(context, args);
    };
    var callNow = immediate && !timeout;
    clearTimeout(timeout);
    timeout = setTimeout(later, wait);
    if (callNow) func.apply(context, args);
  };
}

const ACTIONS = {
  EXECUTE_ACTION: {
    id: 'yup-execute-action',
    label: 'Execute Statement'
  }
};


const EDITOR_CONTENTS = {
  initialValue: `
{
  "find": "",
  "filter": {}
}
`.trim(),
  setContent(collectionName) {
    return `
{
  "find": "${collectionName}",
  "filter": {}
}
`.trim();
  }
};

require.config({ paths: { 'vs': 'monaco/dev/vs' } });
require(['vs/editor/editor.main'], function () {
  const editor = monaco.editor.create(document.getElementById('container'), {
    value: EDITOR_CONTENTS.initialValue,
    language: 'json',
    roundedSelection: false,
    scrollBeyondLastLine: false,
    theme: "vs-dark",
    automaticLayout: true
  });

  editor.onKeyUp(debounce(function () {
    window.external.notify(`KeyUpInEditor;${editor.getValue({ lineEnding: 'lf', preserveBOM: true }).trim()}`);
  }, 250));
  editor.addAction({
    id: ACTIONS.EXECUTE_ACTION.id,
    label: ACTIONS.EXECUTE_ACTION.label,
    // An optional array of keybindings for the action.
    keybindings: [
      monaco.KeyMod.CtrlCmd | monaco.KeyCode.Enter
    ],
    precondition: null,
    keybindingContext: null,
    contextMenuGroupId: 'statemenets',
    contextMenuOrder: 1.5,
    // Method that will be executed when the action is triggered.
    // @param editor The editor instance is passed in as a convinience
    run() {
      window.external.notify(`ExecuteInEditor;${editor.getValue({ lineEnding: 'lf', preserveBOM: true }).trim()}`);
      return null;
    }
  });

  window.setEditorContent = function setEditorContent(entry) {
    try {
      var entryObj = JSON.parse(entry);
    } catch (e) {
      return ["Error", "Entry is not a Json Object"];
    }

    editor.setValue(EDITOR_CONTENTS.setContent(entryObj.Name));
    window.external.notify(`ExecuteInEditor;${editor.getValue({ lineEnding: 'lf', preserveBOM: true }).trim()}`);
    return ["Success"];
  };
});
