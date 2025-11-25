String.prototype.appendUniqueId = function(this: string): string {
  const uniqueId = Math.random().toString(36).substring(2);
  return `${this}-${uniqueId}`;
};