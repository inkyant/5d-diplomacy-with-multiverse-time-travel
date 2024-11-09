import { useContext, useRef } from 'react';
import Button from './common/Button';
import OrderEntryContext from '../context/OrderEntryContext';
import { OrderEntryActionType } from '../../types/context/orderEntryAction';
import WorldContext from '../context/WorldContext';
import TextInput from './common/TextInput';
import { parseOrder } from '../../types/str_to_orders';

const SubmitButton = () => {
  const { world, submitOrders, isLoading, error } = useContext(WorldContext);
  const { dispatch, orders } = useContext(OrderEntryContext);
  const textRef = useRef<string>("")

  const onSubmit = () => {
    dispatch({ $type: OrderEntryActionType.Submit });

    const newOrders = textRef.current.split(";").map((str) => {
      str = str.trim()
      if (str.length < 2) return
      const [country, country_orders] = [str.split(":")[0], str.split(":")[1]]
      return country_orders.split(",").map((ord) => {
        return parseOrder(ord, country)
      }).filter((value) => value != undefined)
    }).filter((value) => value != undefined).flat()

    if (newOrders && textRef.current.length > 3) {
      console.log(newOrders)
      submitOrders(newOrders);
    } else {
      submitOrders(orders);
    }
  };

  const onChange = (value: string) => {
    textRef.current = value
  }

  return (
    <div className="absolute right-10 bottom-10">
      <TextInput placeholder='input moves' onChange={onChange}></TextInput>
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
