import { useContext } from 'react';
import Phase from '../../../types/enums/phase';
import {
  boardBorderWidth,
  boardSeparation,
  majorBoardWidth,
  minorBoardWidth,
  pastTurnOpacity,
} from '../../../utils/constants';
import Map from './Map';
import BoardData, { getBoardName } from '../../../types/board';
import Adjustment from './Adjustment';
import Nation, { getNationColour } from '../../../types/enums/nation';
import colours from '../../../utils/colours';
import OrderEntryContext from '../../context/OrderEntryContext';
import { OrderType } from '../../../types/order';
import WorldContext from '../../context/WorldContext';

type BoardProps = {
  board: BoardData;
  winner: Nation | null;
  isActive: boolean;
};

const Board = ({ board, winner, isActive }: BoardProps) => {
  const { phase } = board;

  const { boardState } = useContext(WorldContext);
  const { currentOrder } = useContext(OrderEntryContext);

  const canMove =
    (isActive && !boardState?.isRetreatTurn) ||
    (currentOrder !== null && currentOrder.$type !== OrderType.Build) ||
    Object.values(board.units).some((unit) => unit.mustRetreat);

  const width = phase === Phase.Winter ? minorBoardWidth : majorBoardWidth;

  return (
    <div
      className="flex-col content-center relative"
      style={{
        minHeight: majorBoardWidth,
        height: majorBoardWidth,
        margin: boardSeparation / 2,
      }}
    >
      <div className="rounded-xl" style={{ backgroundColor: colours.boardBackground }}>
        <div
          className="relative rounded-xl"
          style={{
            backgroundColor: colours.boardBackground,
            minWidth: width,
            width,
            minHeight: width,
            height: width,
            borderWidth: boardBorderWidth,
            borderColor: winner ? getNationColour(winner) : colours.boardBorder,
            boxShadow: winner && isActive ? `0px 0px 100px 50px ${getNationColour(winner)}` : '',
          }}
        >
          <p className="text-md absolute z-10 -mt-7">{getBoardName(board)}</p>
          <Map board={board} />
          {!canMove && (
            <div
              className="absolute w-full h-full top-0 z-10"
              style={{
                backgroundColor: colours.boardBackground,
                opacity: 1 - pastTurnOpacity,
              }}
            />
          )}
        </div>
      </div>
      {phase === Phase.Winter && <Adjustment board={board} isActive={isActive} />}
    </div>
  );
};

export default Board;
