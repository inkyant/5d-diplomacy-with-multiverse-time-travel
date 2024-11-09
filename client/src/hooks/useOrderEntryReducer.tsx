import { useContext, useEffect, useMemo, useReducer } from 'react';
import InputMode from '../types/enums/inputMode';
import { OrderStatus, OrderType, getOrderKey } from '../types/order';
import OrderEntryAction, {
  AddOrderAction,
  OrderEntryActionType,
  SetInputModeAction,
} from '../types/context/orderEntryAction';
import OrderEntryState from '../types/context/orderEntryState';
import UnitType from '../types/enums/unitType';
import { compareLocations, getLocationKey } from '../types/location';
import Phase from '../types/enums/phase';
import regions from '../data/regions';
import GameContext from '../components/context/GameContext';
import WorldContext from '../components/context/WorldContext';

const handleAdjustmentOrderCreation = (
  state: Omit<OrderEntryState, 'dispatch'>,
  action: AddOrderAction,
): Omit<OrderEntryState, 'dispatch'> => {
  const { player, currentMode, currentOrder } = state;
  const { unit, location } = action;
  const filteredOrders = state.orders.filter(
    (order) => !compareLocations(order.location, action.location, true),
  );
  const nation = regions[location.region].homeNation;

  const isPlayerUnit = unit && (!player || unit.owner === player);
  const isPlayerNation = nation && (!player || nation === player);

  // Beginning a build order
  if (
    (currentMode === InputMode.Build || (!unit && currentMode === InputMode.None)) &&
    !currentOrder
  ) {


    return {
      ...state,
      currentOrder: {
        $type: OrderType.Build,
        status: OrderStatus.New,
        location,
        unit: null,
      },
      orders: filteredOrders,
      currentMode: InputMode.Build,
    };
  }

  // Ending a build order
  if (
    currentMode === InputMode.Build &&
    currentOrder?.$type === OrderType.Build &&
    compareLocations(currentOrder.location, location)
  ) {
    if (!unit) return state;

    return {
      ...state,
      orders: [
        ...filteredOrders,
        {
          ...currentOrder,
          location:
            action.unit?.type === UnitType.Army
              ? { ...currentOrder.location, region: currentOrder.location.region.split('_')[0] }
              : currentOrder.location,
          unit: {
            owner: unit.owner,
            type: unit.type,
            mustRetreat: false,
          },
        },
      ],
      currentMode: InputMode.None,
      currentOrder: null,
    };
  }

  // Setting a disband order
  if (currentMode === InputMode.Disband || (unit && currentMode === InputMode.None)) {
    if (!isPlayerUnit) return state;

    return {
      ...state,
      orders: [
        ...filteredOrders,
        {
          $type: OrderType.Disband,
          status: OrderStatus.New,
          unit,
          location,
        },
      ],
      currentMode: InputMode.None,
      currentOrder: null,
    };
  }

  return state;
};

const handleBasicOrderCreation = (
  state: Omit<OrderEntryState, 'dispatch'>,
  action: AddOrderAction,
): Omit<OrderEntryState, 'dispatch'> => {
  const { player, currentMode, currentOrder, orders } = state;
  const { unit, location } = action;
  const filteredOrders = orders.filter(
    (order) => !compareLocations(order.location, action.location),
  );

  const isPlayerUnit = unit && (!player || unit.owner === player);

  // Beginning a move order
  if (
    currentMode === InputMode.None ||
    currentMode === InputMode.Build ||
    (currentMode === InputMode.Move && !currentOrder)
  ) {
    if (!isPlayerUnit) return state;

    return {
      ...state,
      currentOrder: {
        $type: OrderType.Move,
        status: unit.mustRetreat ? OrderStatus.RetreatNew : OrderStatus.New,
        unit,
        location,
        destination: null,
      },
      currentMode: InputMode.Move,
      orders: filteredOrders,
    };
  }

  // Setting a hold order
  if (currentMode === InputMode.Hold) {
    if (!isPlayerUnit) return state;

    return {
      ...state,
      orders: [
        ...filteredOrders,
        {
          $type: OrderType.Hold,
          status: unit.mustRetreat ? OrderStatus.RetreatNew : OrderStatus.New,
          unit,
          location,
        },
      ],
      currentMode: InputMode.None,
      currentOrder: null,
    };
  }

  // Ending a move order
  if (currentMode === InputMode.Move && currentOrder?.$type === OrderType.Move) {
    if (compareLocations(currentOrder.location, location)) {
      return {
        ...state,
        orders: [
          ...orders,
          {
            ...currentOrder,
            $type: OrderType.Hold,
          },
        ],
        currentMode: InputMode.None,
        currentOrder: null,
      };
    }

    return {
      ...state,
      orders: [
        ...orders,
        {
          ...currentOrder,
          destination: location,
        },
      ],
      currentMode: InputMode.None,
      currentOrder: null,
    };
  }

  // Beginning a support order
  if (currentMode === InputMode.Support && !currentOrder) {
    if (!isPlayerUnit) return state;

    return {
      ...state,
      currentOrder: {
        $type: OrderType.Support,
        status: unit.mustRetreat ? OrderStatus.RetreatNew : OrderStatus.New,
        unit,
        location,
        destination: null,
        supportLocation: null,
      },
      orders: filteredOrders,
    };
  }

  // Enriching a support order
  if (
    currentMode === InputMode.Support &&
    currentOrder?.$type === OrderType.Support &&
    !currentOrder.supportLocation
  ) {
    return {
      ...state,
      currentOrder: {
        ...currentOrder,
        supportLocation: location,
      },
    };
  }

  // Ending a support order
  if (
    currentMode === InputMode.Support &&
    currentOrder?.$type === OrderType.Support &&
    currentOrder.supportLocation
  ) {
    return {
      ...state,
      orders: [
        ...orders,
        {
          ...currentOrder,
          destination: location,
        },
      ],
      currentMode: InputMode.None,
      currentOrder: null,
    };
  }

  // Beginning a convoy order
  if (currentMode === InputMode.Convoy && !currentOrder) {
    if (!isPlayerUnit || unit.type !== UnitType.Fleet) return state;

    return {
      ...state,
      currentOrder: {
        $type: OrderType.Convoy,
        status: unit.mustRetreat ? OrderStatus.RetreatNew : OrderStatus.New,
        unit,
        location,
        destination: null,
        convoyLocation: null,
      },
      orders: filteredOrders,
    };
  }

  // Enriching a convoy order
  if (
    currentMode === InputMode.Convoy &&
    currentOrder?.$type === OrderType.Convoy &&
    !currentOrder.convoyLocation
  ) {
    return {
      ...state,
      currentOrder: {
        ...currentOrder,
        convoyLocation: location,
      },
    };
  }

  // Ending a convoy order
  if (
    currentMode === InputMode.Convoy &&
    currentOrder?.$type === OrderType.Convoy &&
    currentOrder.convoyLocation
  ) {
    return {
      ...state,
      orders: [
        ...orders,
        {
          ...currentOrder,
          destination: location,
        },
      ],
      currentMode: InputMode.None,
      currentOrder: null,
    };
  }

  // Setting a disband order (for retreats)
  if (currentMode === InputMode.Disband && unit?.mustRetreat) {
    if (!isPlayerUnit) return state;

    return {
      ...state,
      orders: [
        ...filteredOrders,
        {
          $type: OrderType.Disband,
          status: OrderStatus.RetreatNew,
          unit,
          location,
        },
      ],
      currentMode: InputMode.None,
      currentOrder: null,
    };
  }

  return state;
};

const handleInputModeChange = (
  state: Omit<OrderEntryState, 'dispatch'>,
  action: SetInputModeAction,
): Omit<OrderEntryState, 'dispatch'> => {
  const { availableModes, currentMode: oldMode, currentOrder, orders } = state;
  const { mode: newMode } = action;

  if (newMode === oldMode || !availableModes.includes(newMode)) return state;

  if (!currentOrder) {
    return {
      ...state,
      currentMode: newMode,
    };
  }

  // Changing a move/support/convoy to a hold halfway through
  if (
    (currentOrder.$type === OrderType.Move ||
      currentOrder.$type === OrderType.Support ||
      currentOrder.$type === OrderType.Convoy) &&
    newMode === InputMode.Hold
  ) {
    return {
      ...state,
      currentMode: InputMode.None,
      currentOrder: null,
      orders: [
        ...orders,
        {
          $type: OrderType.Hold,
          status: currentOrder.status,
          unit: currentOrder.unit,
          location: currentOrder.location,
        },
      ],
    };
  }

  // Changing a move to a support halfway through
  if (currentOrder.$type === OrderType.Move && newMode === InputMode.Support) {
    return {
      ...state,
      currentMode: InputMode.Support,
      currentOrder: {
        ...currentOrder,
        $type: OrderType.Support,
        supportLocation: null,
      },
    };
  }

  // Changing a move to a convoy halfway through
  if (currentOrder.$type === OrderType.Move && newMode === InputMode.Convoy) {
    return {
      ...state,
      currentMode: InputMode.Convoy,
      currentOrder: {
        ...currentOrder,
        $type: OrderType.Convoy,
        convoyLocation: null,
      },
    };
  }

  // Wiping the current order
  return {
    ...state,
    currentMode: newMode,
    currentOrder: null,
  };
};

const orderEntryReducer = (
  state: Omit<OrderEntryState, 'dispatch'>,
  action: OrderEntryAction,
): Omit<OrderEntryState, 'dispatch'> => {
  switch (action.$type) {
    case OrderEntryActionType.LoadWorld:
      return {
        ...state,
        orders: [],
        player: action.player,
        currentMode: InputMode.None,
        currentOrder: null,
      };
    case OrderEntryActionType.Submit:
      return {
        ...state,
        currentMode: InputMode.None,
        currentOrder: null,
      };
    case OrderEntryActionType.SetMode:
      return handleInputModeChange(state, action);
    case OrderEntryActionType.SetAvailableModes:
      return {
        ...state,
        availableModes: action.modes,
        currentMode: action.modes.includes(state.currentMode) ? state.currentMode : InputMode.None,
      };
    case OrderEntryActionType.Remove:
      return {
        ...state,
        orders: state.orders.filter((order) => !compareLocations(order.location, action.location)),
        highlightedOrder: null,
      };
    case OrderEntryActionType.Add:
      return action.location.phase === Phase.Winter
        ? handleAdjustmentOrderCreation(state, action)
        : handleBasicOrderCreation(state, action);
    case OrderEntryActionType.HighlightStart:
      return {
        ...state,
        highlightedOrder:
          state.orders.find(
            (order) =>
              getOrderKey(order) === `${getLocationKey(action.location)} ${OrderStatus.New}` ||
              getOrderKey(order) === `${getLocationKey(action.location)} ${OrderStatus.RetreatNew}`,
          ) ?? null,
      };
    case OrderEntryActionType.HighlightStop:
      return {
        ...state,
        highlightedOrder: null,
      };
    default:
      return state;
  }
};

export const initialOrderEntryState: OrderEntryState = {
  player: null,
  orders: [],
  currentMode: InputMode.None,
  currentOrder: null,
  highlightedOrder: null,
  availableModes: [InputMode.None],
  dispatch: () => {},
};

const useOrderEntryReducer = () => {
  const [state, dispatch] = useReducer(orderEntryReducer, initialOrderEntryState);
  const { game } = useContext(GameContext);
  const { world } = useContext(WorldContext);

  useEffect(
    () =>
      dispatch({
        $type: OrderEntryActionType.LoadWorld,
        player: game?.player ?? null,
      }),
    [world?.iteration, game?.player],
  );

  return useMemo(() => ({ ...state, dispatch }), [state, dispatch]);
};

export default useOrderEntryReducer;
