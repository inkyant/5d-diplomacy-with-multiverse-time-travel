import { useContext, useRef } from 'react';
import Button from './common/Button';
import OrderEntryContext from '../context/OrderEntryContext';
import { OrderEntryActionType } from '../../types/context/orderEntryAction';
import WorldContext from '../context/WorldContext';
import TextInput, { TextInputHandle } from './common/TextInput';
import { orderToString, stringToOrders } from '../../types/str_to_orders';

const SubmitButton = () => {
  const { world, submitOrders, isLoading, error } = useContext(WorldContext);
  const { dispatch, orders } = useContext(OrderEntryContext);
  const textRef = useRef<string>("")
  const inputRef = useRef<TextInputHandle>(null);

  const onSubmit = () => {
    dispatch({ $type: OrderEntryActionType.Submit });

    const newOrders = stringToOrders(textRef.current)

    if (newOrders && textRef.current.length > 3) {
      console.log(newOrders)
      submitOrders(newOrders);
    } else {
      console.log(orderToString(orders))
      submitOrders(orders);
    }

    inputRef.current?.clear()
  };

  const onChange = (value: string) => {
    textRef.current = value
  }

  return (
    <div className="absolute right-10 bottom-10">
      <TextInput placeholder='input moves' onChange={onChange} ref={inputRef}></TextInput>
      <Button
        text="Submit"
        onClick={onSubmit}
        minWidth={200}
        isDisabled={isLoading || error !== null}
        isBusy={world !== null && !error && isLoading}
      />
    </div>
  );
};

export default SubmitButton;
