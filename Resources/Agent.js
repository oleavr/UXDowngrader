'use strict';

Interceptor.attach(Module.findExportByName('user32.dll', 'DrawTextExW'), {
  onEnter(args) {
    const originalText = Memory.readUtf16String(args[1], args[2].toInt32());
    if (originalText === null)
      return;
    const transformedText = transform(originalText);
    Memory.writeUtf16String(args[1], transformedText);
    args[2] = ptr(transformedText.length);
  }
});

Interceptor.attach(Module.findExportByName('gdi32.dll', 'ExtTextOutW'), {
  onEnter(args) {
    const originalText = Memory.readUtf16String(args[5], args[6].toInt32());
    if (originalText === null)
      return;
    const transformedText = transform(originalText);
    Memory.writeUtf16String(args[5], transformedText);
    args[6] = ptr(transformedText.length);
  }
});

function transform(str) {
  return str.toUpperCase();
}
