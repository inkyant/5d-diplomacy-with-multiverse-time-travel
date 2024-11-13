import { useState, forwardRef, useImperativeHandle } from 'react';

type TextInputProps = {
  placeholder: string;
  onChange: (value: string) => void;
};

export type TextInputHandle = {
  clear: () => void;
};

const TextInput = forwardRef<TextInputHandle, TextInputProps>(({ placeholder, onChange }, ref) => {
  const [value, setValue] = useState('');

  useImperativeHandle(ref, () => ({
    clear: () => {
      setValue('');
      onChange('');
    }
  }));

  return (
    <input
      type="text"
      className="w-64 h-16 p-4 border-4 rounded-xl text-lg"
      placeholder={placeholder}
      value={value}
      onChange={(event) => {
        setValue(event.target.value);
        onChange(event.target.value);
      }}
    />
  );
});

export default TextInput;
