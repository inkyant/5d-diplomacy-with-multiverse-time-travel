
import Location from './location';
import Phase from './enums/phase';
import UnitType from './enums/unitType';
import Order, { OrderStatus, OrderType } from './order';
import Unit from './unit';
import Nation from './enums/nation';


function parseOrder(orderString: string, country: string) {

  const nation = Object.values(Nation).find(
    nation => nation.toLowerCase() === country.toLowerCase()
  );

  if (!nation) {
    console.error("Could not find nation", country)
    return
  }

  orderString = orderString.trim()
  const parts = orderString.split(' ');

  if (parts.length < 4) {
    console.error("too few order parameters provided for ", country, "'s order of ", orderString)
    return
  }

  // Parse common Location data
  const timeline = parseInt(parts[0].substring(1));
  const phase = parts[1].charAt(0);
  const year = 1900 + parseInt(parts[1].substring(1));

  const location: Location = {
    timeline,
    year,
    phase: phase === 'S' ? Phase.Spring : phase === 'F' ? Phase.Fall : Phase.Winter,
    region: ''
  };

  // For Build and Disband orders in Winter phase
  if (phase === 'W') {
    if (parts[2] === 'D') {
      location.region = parts[4]
      return parseDisband(parseUnit(nation, parts[3]), location);
    }
    location.region = parts[3]
    return parseBuild(parseUnit(nation, parts[2]), location);
  }

  // For other orders, standard format
  location.region = parts[3];
  const unit = parseUnit(nation, parts[2])

  if (parts[4] === "H") {
    return parseHold(unit, location)
  }

  // Determine order type and parse accordingly
  switch (parts[4]) {
    case '->':
      const dest = parseDestination(orderString.split("->")[1], location)
      return parseMove(unit, location, dest);
    case 'S':

      const supportStr = orderString.split(" ").slice(5).join(" ")
      let supportFrom: Location
      let supportTo: Location
      if (supportStr.includes("->")) {
        const moveParts = supportStr.split("->")
        supportFrom = parseDestination(moveParts[0], location)
        supportTo = parseDestination(moveParts[1], location)
      } else {
        supportFrom = parseDestination(supportStr.replace(/\sH\s|\sH$/, '').trim(), location)
        supportTo = supportFrom
      }

      return parseSupport(unit, location, supportFrom, supportTo);
    case 'C':
      
      const convoyStr = orderString.split(" ").slice(5).join(" ")
      const convoyFrom = parseDestination(convoyStr.split("->")[0], location)
      const convoyTo = parseDestination(convoyStr.split("->")[1], location)

      return parseConvoy(unit, location, convoyFrom, convoyTo);
    case 'D':
      return parseDisband(unit, location)
    default:
      console.error("Invalid order type")
      return
  }
}

function parseDestination(destination_str: string, default_location: Location): Location {
  const destination_parts = destination_str.trim().split(" ")

  const destination: Location = {
    timeline: default_location.timeline,
    year: default_location.year,
    phase: default_location.phase,
    region: destination_parts[destination_parts.length - 1]
  }

  if (destination_parts.length == 3) {
    destination.timeline = parseInt(destination_parts[0].substring(1));
    destination.phase = destination_parts[1].charAt(0) === "S" ? Phase.Spring : Phase.Fall;
    destination.year = 1900 + parseInt(destination_parts[1].substring(1));      
  }

  return destination
}

function parseUnit(nation: Nation, type: string): Unit {
  return {
    owner: nation,
    type: type === "F" ? UnitType.Fleet : UnitType.Army,
    mustRetreat: false
  }
}

function parseHold(unit: Unit, location: Location): Order {
  return {
    $type: OrderType.Hold,
    status: OrderStatus.New,
    unit,
    location
  };
}

function parseMove(unit: Unit, location: Location, destination: Location): Order {
  return {
    $type: OrderType.Move,
    status: OrderStatus.New,
    unit,
    location,
    destination: destination
  };
}

function parseSupport(unit: Unit, location: Location, supportFrom: Location, supportTo: Location): Order {
  return {
    $type: OrderType.Support,
    status: OrderStatus.New,
    unit,
    location,
    supportLocation: supportFrom,
    destination: supportTo
  };
}

function parseConvoy(unit: Unit, location: Location, convoyFrom: Location, convoyTo: Location): Order {
  return {
    $type: OrderType.Convoy,
    status: OrderStatus.New,
    unit,
    location,
    convoyLocation: convoyFrom,
    destination: convoyTo
  };
}

function parseBuild(unit: Unit, location: Location): Order {
  return {
    $type: OrderType.Build,
    status: OrderStatus.New,
    unit: unit,
    location: location
  };
}

function parseDisband(unit: Unit, location: Location): Order {
  return {
    $type: OrderType.Disband,
    status: OrderStatus.New,
    unit: unit,
    location: location
  };
}


export function orderToString(orders: Order[]) {
  const grouped = orders.reduce<{ [key: string]: Order[] }>((acc, order) => {
    if (order.unit?.owner) {
      if (!acc[order.unit?.owner]) {
        acc[order.unit?.owner] = [];
      }
      acc[order.unit?.owner].push(order)
    }
    return acc
  }, {});

  let orderStr = ""

  const formatLocation = (loc: Location | null) => {
    if (!loc) return ""
    return "T" + loc.timeline + " " + loc.phase[0] + ('0' + (loc.year - 1900)).slice(-2)
  }

  Object.values(grouped).forEach(orderList => {

    orderStr += orderList[0].unit?.owner + ": "

    orderList.forEach(order => {
      orderStr += formatLocation(order.location) + " " + order.unit?.type[0] + " " + order.location.region
    
      if (order.$type == OrderType.Hold) {
        orderStr += " H"
      } else if (order.$type == OrderType.Move) {
        if (order.destination == null) {
          orderStr += " H"
        } else {
          orderStr += " -> " + formatLocation(order.destination) + " " + order.destination.region
        }
      } else if (order.$type == OrderType.Support) {
        orderStr += " S " + formatLocation(order.supportLocation) + " " + order.supportLocation?.region
        if (order.destination == null || JSON.stringify(order.supportLocation) == JSON.stringify(order.destination)) {
          orderStr += " H"
        } else {
          orderStr += " -> " + formatLocation(order.destination) + " " + order.destination.region
        }
      } else if (order.$type == OrderType.Convoy) {
        orderStr += " C " + formatLocation(order.convoyLocation) + " " + order.convoyLocation?.region + " -> " + formatLocation(order.destination) + " " + order.destination?.region
      } else if (order.$type == OrderType.Disband) {
        orderStr += " D"
      }

      orderStr += ", "
    })

    orderStr = orderStr.slice(0, -2) + "; "

  })
  return orderStr
}

export function stringToOrders(str: string): Order[] {

  // have to do this because typescript was being weird and not recognizing filter()
  let rtnOrders: Order[] = []

  str.split(";").map((str) => {
    str = str.trim()
    if (str.length < 2) return
    const [country, countryOrdersStr] = [str.split(":")[0], str.split(":")[1]]

    let countryOrders: Order[] = []
    countryOrdersStr.split(",").map((ord) => {
      return parseOrder(ord, country)
    }).forEach(v => (v !== undefined) && countryOrders.push(v))
    return countryOrders
  }).flat().forEach(v => (v !== undefined) && rtnOrders.push(v))

  return rtnOrders
}