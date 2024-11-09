import { useContext, useRef } from 'react';
import Button from './common/Button';
import OrderEntryContext from '../context/OrderEntryContext';
import { OrderEntryActionType } from '../../types/context/orderEntryAction';
import WorldContext from '../context/WorldContext';
import TextInput from './common/TextInput';
import { parseOrder } from '../../types/str_to_orders';
import Order from '../../types/order';

const SubmitButton = () => {
  const { world, submitOrders, isLoading, error } = useContext(WorldContext);
  const { dispatch, orders } = useContext(OrderEntryContext);
  const textRef = useRef<Order[] | null>(null)

  const onSubmit = () => {
    dispatch({ $type: OrderEntryActionType.Submit });
    submitOrders(textRef.current ? textRef.current : orders);
  };

  const onChange = (value: string) => {
    const newOrders = value.split(";").map((str) => {
      return parseOrder(str.split(":")[1], str.split(":")[0])
    }).filter((value) => value != undefined)
    if (newOrders)
      textRef.current = newOrders
  }

  return (
    <div className="absolute right-10 bottom-10">
      {/* <TextInput placeholder='input moves' onChange={onChange}></TextInput> */}
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
